using Microsoft.Extensions.Logging;
using ShippingRates.Models;
using ShippingRates.Models.Usps;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShippingRates.Services.Usps;

internal class UspsShippingOptionsService : UspsPricesService
{
    internal UspsShippingOptionsService(ILogger? logger) : base(logger)
    {
    }

    public async Task<UspsShippingOptionsResponse?> GetShippingOptions(
        HttpClient httpClient,
        string token,
        UspsShippingOptionsRequest request,
        bool isProduction,
        RateResultAggregator resultBuilder)
    {
        var uri = new Uri(new Uri(GetBaseUri(isProduction)), "/shipments/v3/options/search");
        return await PostAsync<UspsShippingOptionsRequest, UspsShippingOptionsResponse>(httpClient, token, uri, request, resultBuilder);
    }
}
