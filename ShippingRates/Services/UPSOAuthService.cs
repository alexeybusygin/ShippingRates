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
        const string OAuthRequestUri = "https://wwwcie.ups.com/security/v1/oauth/token";

        public static async Task<string> GetTokenAsync(UPSProviderConfiguration configuration, HttpClient httpClient, Action<Error> reportError)
        {
            var token = TokenCacheService.GetToken(configuration.ClientId);
            if (!string.IsNullOrEmpty(token))
                return token;

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, OAuthRequestUri);
            requestMessage.Headers.Add("x-merchant-id", configuration.AccountNumber);

            var base64clientString = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{configuration.ClientId}:{configuration.ClientSecret}"));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64clientString);

            var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };
            requestMessage.Content = new FormUrlEncodedContent(postData);

            var request = await httpClient.SendAsync(requestMessage);
            var response = await request.Content.ReadAsStringAsync();

            if (request.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<UPSOAuthResponse>(response);

                TokenCacheService.AddToken(configuration.ClientId, result.AccessToken, result.ExpiresIn);

                return result.AccessToken;
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<UPSOAuthErrorResponse>(response);
                if ((errorResponse?.Response?.Errors?.Length ?? 0) > 0)
                {
                    foreach (var error in errorResponse.Response.Errors)
                    {
                        reportError(new Error()
                        {
                            Number = error.Code,
                            Description = error.Message
                        });
                    }
                }
                else
                {
                    reportError(new Error() { Description = $"Unknown error while fetching UPS OAuth token: {request.StatusCode} {response}" });
                }

                return null;
            }
        }

        public class UPSOAuthResponse
        {
            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }
            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
        }

        public class UPSOAuthErrorResponse
        {
            [JsonPropertyName("response")]
            public UPSOAuthErrorResponseBody Response { get; set; }
        }

        public class UPSOAuthErrorResponseBody
        {
            [JsonPropertyName("errors")]
            public UPSOAuthErrorItem[] Errors { get; set; }
        }

        public class UPSOAuthErrorItem
        {
            [JsonPropertyName("code")]
            public string Code { get; set; }
            [JsonPropertyName("message")]
            public string Message { get; set; }
        }
    }
}
