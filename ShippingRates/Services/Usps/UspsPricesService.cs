using Microsoft.Extensions.Logging;
using ShippingRates.Models;
using ShippingRates.Models.Usps;
using ShippingRates.ShippingProviders.Usps;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShippingRates.Services.Usps;

internal class UspsPricesService
{
    private readonly ILogger? _logger;

    internal UspsPricesService(ILogger? logger)
    {
        _logger = logger;
    }

    protected static string GetBaseUri(bool isProduction)
        => $"https://{(isProduction ? "apis" : "apis-tem")}.usps.com";

    static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<UspsPricesResponse?> GetDomesticPrices(
        HttpClient httpClient,
        string token,
        UspsDomesticRequest request,
        bool isProduction,
        RateResultAggregator resultBuilder)
    {
        var uri = new Uri(new Uri(GetBaseUri(isProduction)), "/prices/v3/total-rates/search");
        return await PostAsync<UspsDomesticRequest, UspsPricesResponse>(httpClient, token, uri, request, resultBuilder);
    }

    public async Task<UspsPricesResponse?> GetInternationalPrices(
        HttpClient httpClient,
        string token,
        UspsInternationalRequest request,
        bool isProduction,
        RateResultAggregator resultBuilder)
    {
        var uri = new Uri(new Uri(GetBaseUri(isProduction)), "/international-prices/v3/total-rates/search");
        return await PostAsync<UspsInternationalRequest, UspsPricesResponse>(httpClient, token, uri, request, resultBuilder);
    }

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(
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

        try
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<TResponse>(response)
                    ?? throw new UspsResponseException(UspsMessages.Error.DeserializationFailed);
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<UspsErrorResponse>(response)
                    ?? throw new UspsResponseException(UspsMessages.Error.DeserializationFailed);

                if (errorResponse.Error != null)
                {
                    resultBuilder.AddProviderError(new Error()
                    {
                        Number = errorResponse.Error.Code,
                        Description = errorResponse.Error.Message
                    });
                    _logger?.LogError(UspsMessages.Error.UspsErrorWithCode, errorResponse.Error.Code, errorResponse.Error.Message);

                    if (errorResponse.Error.Errors != null)
                    {
                        foreach (var error in errorResponse.Error.Errors)
                        {
                            resultBuilder.AddProviderError(new Error()
                            {
                                Number = error.Code,
                                Description = error.Title,
                                Source = error.Source?.Parameter
                            });
                            _logger?.LogError(UspsMessages.Error.UspsErrorWithCode, error.Code, error.Detail);
                        }
                    }
                }

                return default;
            }
        }
        catch (UspsResponseException ex)
        {
            resultBuilder.AddInternalError(UspsMessages.Error.UspsError, ex.Message);
            _logger?.LogError(UspsMessages.Error.UspsError, ex.Message);
        }
        catch (Exception ex)
        {
            resultBuilder.AddInternalError(UspsMessages.Error.UnknownError, ex.Message, responseMessage.StatusCode, response);
            _logger?.LogError(UspsMessages.Error.UnknownError, ex.Message, responseMessage.StatusCode, response);
        }

        return default;
    }
}
