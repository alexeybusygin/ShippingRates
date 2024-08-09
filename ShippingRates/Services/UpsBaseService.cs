using ShippingRates.Models.UPS;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShippingRates.Services
{
    internal class UpsBaseService
    {
        static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        protected static async Task<TResponse> PostAsync<TRequest, TResponse>(HttpClient httpClient, string token, Uri uri, TRequest request, Action<Error> reportError)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonRequest = JsonSerializer.Serialize(request, _jsonSerializerOptions);
            requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var responseMessage = await httpClient.SendAsync(requestMessage);
            var response = await responseMessage.Content.ReadAsStringAsync();

            if (responseMessage.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<TResponse>(response);
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<UpsErrorResponse>(response);
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

                return default;
            }
        }
    }
}
