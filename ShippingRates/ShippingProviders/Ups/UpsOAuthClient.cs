using Microsoft.Extensions.Logging;
using ShippingRates.Models;
using ShippingRates.Models.Ups;
using ShippingRates.Services.OAuth;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShippingRates.ShippingProviders.Ups
{
    internal sealed class UpsOAuthClient : OAuthClientBase<UPSProviderConfiguration>
    {
        public UpsOAuthClient(ILogger logger)
            : base(logger)
        {
        }
        protected override string GetOAuthRequestUri(bool isProduction)
            => $"https://{(isProduction ? "onlinetools" : "wwwcie")}.ups.com/security/v1/oauth/token";

        protected override string ServiceName => "UPS";

        protected override void SetPayload(HttpRequestMessage requestMessage, UPSProviderConfiguration configuration)
        {
            requestMessage.Headers.Add("x-merchant-id", configuration.AccountNumber);

            var basicCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{configuration.ClientId}:{configuration.ClientSecret}"));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicCredentials);

            var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };
            requestMessage.Content = new FormUrlEncodedContent(postData);
        }

        protected override TokenResponse ParseResponse(string response)
        {
            var result = JsonSerializer.Deserialize<UPSOAuthResponse>(response);
            return new TokenResponse
            {
                AccessToken = result.AccessToken,
                ExpiresIn = int.TryParse(result.ExpiresIn, out var expiresIn) ? expiresIn : 0 // Comes as a string, so need to parse it
            };
        }

        protected override bool ParseError(string response, RateResultAggregator resultAggregator)
        {
            var errorResponse = JsonSerializer.Deserialize<UpsErrorResponse>(response);
            if ((errorResponse?.Response?.Errors?.Length ?? 0) == 0)
                return false;

            foreach (var error in errorResponse.Response.Errors)
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
