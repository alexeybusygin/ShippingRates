using ShippingRates.Models.UPS;
using ShippingRates.ShippingProviders;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ShippingRates.Services
{
    internal class UPSRatingService
    {
        const string Version = "v2403";

        static string GetRequestUri(string requestOption) => $"https://wwwcie.ups.com/api/rating/{Version}/{requestOption}";

        public static async Task<UPSRatingResponse> GetRatingAsync(HttpClient httpClient, string token, UPSRatingRequest request, Action<Error> reportError)
        {
            request = request ?? throw new ArgumentNullException(nameof(request));

            var requestOption = !string.IsNullOrEmpty(request.RateRequest?.Shipment?.Service?.Code)
                ? "Rate"
                : "Shop";

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, GetRequestUri(requestOption));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonRequest = JsonSerializer.Serialize(request);
            requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var responseMessage = await httpClient.SendAsync(requestMessage);
            var response = await responseMessage.Content.ReadAsStringAsync();

            if (responseMessage.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<UPSRatingResponse>(response);
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
