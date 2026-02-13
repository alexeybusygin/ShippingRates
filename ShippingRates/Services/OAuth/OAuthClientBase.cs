using Microsoft.Extensions.Logging;
using ShippingRates.Models;
using ShippingRates.Models.OAuth;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ShippingRates.Services.OAuth;

internal abstract class OAuthClientBase<TConfiguration>(ILogger? logger)
    where TConfiguration : IOAuthConfiguration
{
    private const int ExpirationBufferSeconds = 30;

    protected ILogger? Logger { get; } = logger;

    protected abstract string GetOAuthRequestUri(bool isProduction);

    protected abstract string ServiceName { get; }

    public async Task<string?> GetTokenAsync(
        TConfiguration configuration,
        HttpClient httpClient,
        RateResultAggregator resultAggregator,
        CancellationToken cancellationToken = default)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
        if (resultAggregator == null) throw new ArgumentNullException(nameof(resultAggregator));

        var clientId = configuration.ClientId ?? throw new ArgumentException(OAuthMessages.Error.ClientIdMissing);

        if (TokenCacheService.TryGetToken(clientId, out var token))
        {
            Logger?.LogDebug(OAuthMessages.Debug.TokenFetchedFromCache, ServiceName, clientId);
            return token;
        }

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, GetOAuthRequestUri(configuration.UseProduction));
        SetPayload(requestMessage, configuration);

        using var responseMessage = await httpClient
            .SendAsync(requestMessage, cancellationToken)
            .ConfigureAwait(false);
        var response = await responseMessage.Content
            .ReadAsStringAsync()
            .ConfigureAwait(false);

        try
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                var tokenResponse = ParseResponse(response);
                if (tokenResponse != null && tokenResponse.AccessToken != null)
                {
                    TokenCacheService.AddToken(clientId, tokenResponse.AccessToken,
                        TimeSpan.FromSeconds(Math.Max(1, tokenResponse.ExpiresIn - ExpirationBufferSeconds)));

                    Logger?.LogDebug(OAuthMessages.Debug.TokenReceived, ServiceName);
                    return tokenResponse.AccessToken;
                }
            }

            if (!ParseError(response, resultAggregator))
            {
                resultAggregator.AddInternalError(OAuthMessages.Error.Unknown, ServiceName, responseMessage.StatusCode, response);
                Logger?.LogError(OAuthMessages.Error.Unknown, ServiceName, responseMessage.StatusCode, response);
            }
        }
        catch (Exception ex)
        {
            resultAggregator.AddInternalError(OAuthMessages.Error.Unknown, ex.Message, responseMessage.StatusCode, response);
            Logger?.LogError(OAuthMessages.Error.Unknown, ex.Message, responseMessage.StatusCode, response);
        }

        return null;
    }

    protected virtual void SetPayload(HttpRequestMessage requestMessage, TConfiguration configuration)
    {
        var payload = new OAuthRequest
        {
            GrantType = "client_credentials",
            ClientId = configuration.ClientId,
            ClientSecret = configuration.ClientSecret
        };
        var payloadString = JsonSerializer.Serialize(payload);
        requestMessage.Content = new StringContent(payloadString, Encoding.UTF8, "application/json");
    }

    protected virtual TokenResponse? ParseResponse(string response)
    {
        var result = JsonSerializer.Deserialize<OAuthResponse>(response);
        if (result == null || result.AccessToken == null) return null;

        return new TokenResponse(result.AccessToken, result.ExpiresIn);
    }

    protected abstract bool ParseError(string response, RateResultAggregator resultAggregator);

    private sealed class OAuthRequest
    {
        [JsonPropertyName("grant_type")]
        public string? GrantType { get; set; }
        [JsonPropertyName("client_id")]
        public string? ClientId { get; set; }
        [JsonPropertyName("client_secret")]
        public string? ClientSecret { get; set; }
    }

    private sealed class OAuthResponse
    {
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }

    protected sealed record TokenResponse(string AccessToken, int ExpiresIn);
}
