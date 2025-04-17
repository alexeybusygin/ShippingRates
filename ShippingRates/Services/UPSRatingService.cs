using Microsoft.Extensions.Logging;
using ShippingRates.Models;
using ShippingRates.Models.UPS;
using ShippingRates.ShippingProviders;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShippingRates.Services
{
    internal class UpsRatingService : UpsBaseService
    {
        const string Version = "v2403";

        internal UpsRatingService(ILogger<UPSProvider> logger) : base(logger)
        {
        }

        static Uri GetRequestUri(bool isRateRequest, bool isProduction)
            => new Uri($"https://{(isProduction ? "onlinetools" : "wwwcie")}.ups.com/api/rating/{Version}/{(isRateRequest ? "Rate" : "Shop")}");

        public async Task<UpsRatingResponse> GetRatingAsync(
            HttpClient httpClient,
            string token,
            bool isProduction,
            UpsRatingRequest request,
            RateResultBuilder resultBuilder)
        {
            request = request ?? throw new ArgumentNullException(nameof(request));

            var isRateRequest = !string.IsNullOrEmpty(request.RateRequest?.Shipment?.Service?.Code);
            var uri = GetRequestUri(isRateRequest, isProduction);

            if (isRateRequest)
            {
                var singleRateResponse = await PostAsync<UpsRatingRequest, UpsSingleRatingResponse>(httpClient, token, uri, request, resultBuilder);
                return singleRateResponse?.GetRatesResponse();
            }
            else
            {
                return await PostAsync<UpsRatingRequest, UpsRatingResponse>(httpClient, token, uri, request, resultBuilder);
            }
        }
    }
}
