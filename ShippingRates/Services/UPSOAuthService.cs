using Microsoft.Extensions.Logging;
using ShippingRates.Models;
using ShippingRates.ShippingProviders;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShippingRates.Services
{
    internal class UPSOAuthService
    {
        private readonly ILogger<UPSProvider> _logger;

        public UPSOAuthService(ILogger<UPSProvider> logger)
        {
            _logger = logger;
        }

        static string GetOAuthRequestUri(bool isProduction)
            => $"https://{(isProduction ? "onlinetools" : "wwwcie")}.ups.com/security/v1/oauth/token";

        public async Task<string> GetTokenAsync(
            UPSProviderConfiguration configuration,
            HttpClient httpClient,
            RateResultBuilder resultBuilder)
        {
            var token = TokenCacheService.GetToken(configuration.ClientId);
            if (!string.IsNullOrEmpty(token))
            {
                _logger?.LogDebug("Fetched UPS token from cache");
                return token;
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, GetOAuthRequestUri(configuration.UseProduction));
            requestMessage.Headers.Add("x-merchant-id", configuration.AccountNumber);

            var base64clientString = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{configuration.ClientId}:{configuration.ClientSecret}"));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64clientString);

            var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };
            requestMessage.Content = new FormUrlEncodedContent(postData);

            var responseMessage = await httpClient.SendAsync(requestMessage);
            var response = await responseMessage.Content.ReadAsStringAsync();

            if (responseMessage.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<UPSOAuthResponse>(response);
                int expiresIn = int.TryParse(result.ExpiresIn, out expiresIn) ? expiresIn : 0;
                                                                // It comes as a string, so need to parse it

                TokenCacheService.AddToken(configuration.ClientId, result.AccessToken, expiresIn);

                _logger?.LogDebug("Received token from UPS API");
                return result.AccessToken;
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<Models.UPS.UpsErrorResponse>(response);
                if ((errorResponse?.Response?.Errors?.Length ?? 0) > 0)
                {
                    foreach (var error in errorResponse.Response.Errors)
                    {
                        resultBuilder.AddError(new Error()
                        {
                            Number = error.Code,
                            Description = error.Message
                        });
                        _logger.LogError("Error while fetching UPS OAuth token: {code} {message}", error.Code, error.Message);
                    }
                }
                else
                {
                    resultBuilder.AddInternalError($"Unknown error while fetching UPS OAuth token: {responseMessage.StatusCode} {response}");
                    _logger.LogError("Unknown error while fetching UPS OAuth token: {statusCode} {response}", responseMessage.StatusCode, response);
                }

                return null;
            }
        }

        class UPSOAuthResponse
        {
            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }
            [JsonPropertyName("expires_in")]
            public string ExpiresIn { get; set; }
        }
    }
}
