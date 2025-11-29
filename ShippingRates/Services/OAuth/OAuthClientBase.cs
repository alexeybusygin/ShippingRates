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

namespace ShippingRates.Services.OAuth
{
    internal abstract class OAuthClientBase<TConfiguration> where TConfiguration : IOAuthConfiguration
    {
        protected readonly ILogger _logger;

        public OAuthClientBase(ILogger logger)
        {
            _logger = logger;
        }

        protected virtual string GetOAuthRequestUri(bool isProduction)
            => throw new NotImplementedException();

        protected virtual string ServiceName => "Service";

        public async Task<string> GetTokenAsync(
            TConfiguration configuration,
            HttpClient httpClient,
            RateResultAggregator resultAggregator,
            CancellationToken cancellationToken = default)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            if (resultAggregator == null) throw new ArgumentNullException(nameof(resultAggregator));

            var token = TokenCacheService.GetToken(configuration.ClientId);
            if (!string.IsNullOrEmpty(token))
            {
                _logger?.LogDebug("Fetched {ServiceName} token from cache for clientId {ClientId}", ServiceName, configuration.ClientId);
                return token;
            }

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, GetOAuthRequestUri(configuration.UseProduction)))
            {
                SetPayload(requestMessage, configuration);

                using (var responseMessage = await httpClient
                    .SendAsync(requestMessage, cancellationToken)
                    .ConfigureAwait(false))
                {
                    var response = await responseMessage.Content
                        .ReadAsStringAsync()
                        .ConfigureAwait(false);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var tokenResponse = ParseResponse(response);

                        TokenCacheService.AddToken(configuration.ClientId, tokenResponse.AccessToken, tokenResponse.ExpiresIn);

                        _logger?.LogDebug("Received OAuth token from {ServiceName}", ServiceName);
                        return tokenResponse.AccessToken;
                    }

                    if (!ParseError(response, resultAggregator))
                    {
                        resultAggregator.AddInternalError($"Unknown error while fetching {ServiceName} OAuth token: {responseMessage.StatusCode} {response}");
                        _logger?.LogError("Unknown error while fetching {ServiceName} OAuth token: {statusCode} {response}", ServiceName, responseMessage.StatusCode, response);
                    }

                    return null;
                }
            }
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

        protected virtual TokenResponse ParseResponse(string response)
        {
            var result = JsonSerializer.Deserialize<OAuthResponse>(response);
            return new TokenResponse
            {
                AccessToken = result.AccessToken,
                ExpiresIn = result.ExpiresIn
            };
        }

        protected virtual bool ParseError(string response, RateResultAggregator resultAggregator)
        {
            var errorResponse = JsonSerializer.Deserialize<Models.FedEx.FedExErrorResponse>(response);
            if ((errorResponse?.Errors?.Length ?? 0) == 0)
                return false;

            foreach (var error in errorResponse.Errors)
            {
                resultAggregator.AddProviderError(new Error()
                {
                    Number = error.Code,
                    Description = error.Message
                });
                _logger?.LogError("Error while fetching {ServiceName} OAuth token: {code} {message}", ServiceName, error.Code, error.Message);
            }
            return true;
        }

        class OAuthRequest
        {
            [JsonPropertyName("grant_type")]
            public string GrantType { get; set; }
            [JsonPropertyName("client_id")]
            public string ClientId { get; set; }
            [JsonPropertyName("client_secret")]
            public string ClientSecret { get; set; }
        }

        class OAuthResponse
        {
            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }
            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
            [JsonPropertyName("scope")]
            public string Scope { get; set; }
        }

        protected class TokenResponse
        {
            public string AccessToken { get; set; }
            public int ExpiresIn { get; set; }
        }
    }
}
