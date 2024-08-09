using ShippingRates.Models.UPS;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShippingRates.Services
{
    internal class UpsRatingService : UpsBaseService
    {
        const string Version = "v2403";

        static Uri GetRequestUri(bool isRateRequest, bool isProduction)
            => new Uri($"https://{(isProduction ? "onlinetools" : "wwwcie")}.ups.com/api/rating/{Version}/{(isRateRequest ? "Rate" : "Shop")}");

        public static async Task<UpsRatingResponse> GetRatingAsync(HttpClient httpClient, string token, bool isProduction, UpsRatingRequest request, Action<Error> reportError)
        {
            request = request ?? throw new ArgumentNullException(nameof(request));

            var isRateRequest = !string.IsNullOrEmpty(request.RateRequest?.Shipment?.Service?.Code);
            var uri = GetRequestUri(isRateRequest, isProduction);

            if (isRateRequest)
            {
                var singleRateResponse = await PostAsync<UpsRatingRequest, UpsSingleRatingResponse>(httpClient, token, uri, request, reportError);
                return singleRateResponse?.GetRatesResponse();
            }
            else
            {
                return await PostAsync<UpsRatingRequest, UpsRatingResponse>(httpClient, token, uri, request, reportError);
            }
        }
    }
}
