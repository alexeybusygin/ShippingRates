using ShippingRates.Models.UPS;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShippingRates.Services
{
    internal class UPSRatingService
    {
        const string Version = "v2403";

        static string GetRequestUri(bool isRateRequest)
            => $"https://wwwcie.ups.com/api/rating/{Version}/{(isRateRequest ? "Rate" : "Shop")}";

        static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public static async Task<UPSRatingResponse> GetRatingAsync(HttpClient httpClient, string token, UPSRatingRequest request, Action<Error> reportError)
        {
            request = request ?? throw new ArgumentNullException(nameof(request));

            var isRateRequest = !string.IsNullOrEmpty(request.RateRequest?.Shipment?.Service?.Code);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, GetRequestUri(isRateRequest));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonRequest = JsonSerializer.Serialize(request, _jsonSerializerOptions);
            requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var responseMessage = await httpClient.SendAsync(requestMessage);
            var response = await responseMessage.Content.ReadAsStringAsync();

            if (responseMessage.IsSuccessStatusCode)
            {
                if (isRateRequest)
                {
                    var singleRateResponse = JsonSerializer.Deserialize<UPSSingleRatingResponse>(response);
                    return singleRateResponse.GetRatesResponse();
                }
                else
                {
                    return JsonSerializer.Deserialize<UPSRatingResponse>(response);
                }
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<UPSErrorResponse>(response);
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
                    reportError(new Error() { Description = $"Unknown error while fetching UPS ratings: {responseMessage.StatusCode} {response}" });
                }

                return null;
            }
        }
    }
}
