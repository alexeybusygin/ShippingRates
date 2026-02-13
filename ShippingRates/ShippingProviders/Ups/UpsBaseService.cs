using Microsoft.Extensions.Logging;
using ShippingRates.Models;
using ShippingRates.Models.Ups;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShippingRates.ShippingProviders.Ups
{
    internal class UpsBaseService
    {
        private readonly ILogger _logger;

        internal UpsBaseService(ILogger logger)
        {
            _logger = logger;
        }

        static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        protected async Task<TResponse> PostAsync<TRequest, TResponse>(
            HttpClient httpClient,
            string token,
            Uri uri,
            TRequest request,
            RateResultAggregator resultBuilder)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonRequest = JsonSerializer.Serialize(request, _jsonSerializerOptions);
            requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            _logger?.LogInformation("Rates Request: {jsonRequest}", jsonRequest);

            var responseMessage = await httpClient.SendAsync(requestMessage);
            var response = await responseMessage.Content.ReadAsStringAsync();

            _logger?.LogInformation("Rates Response: {response}", response);

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
                        resultBuilder.AddProviderError(new Error()
                        {
                            Number = error.Code,
                            Description = error.Message
                        });
                        _logger?.LogError("UPS Error: {code} {message}", error.Code, error.Message);
                    }
                }
                else
                {
                    resultBuilder.AddInternalError("Unknown error while fetching UPS ratings: {responseMessage.StatusCode} {response}");
                    _logger?.LogError("Unknown error while fetching UPS ratings: {statusCode} {response}", responseMessage.StatusCode, response);
                }

                return default;
            }
        }
    }
}
