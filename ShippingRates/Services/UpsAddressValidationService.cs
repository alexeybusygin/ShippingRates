using ShippingRates.Models.UPS;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShippingRates.Services
{
    internal class UpsAddressValidationService : UpsBaseService
    {
        const string Version = "v2";

        const string AddressValidationAndClassification = "3";

        static Uri GetRequestUri(bool isProduction)
            => new Uri($"https://{(isProduction ? "onlinetools" : "wwwcie")}.ups.com/api/addressvalidation/{Version}/{AddressValidationAndClassification}?maximumcandidatelistsize=0");

        public static async Task<UpsRatingResponse> ValidateAsync(HttpClient httpClient, string token, bool isProduction, UpsAddressValidationRequest request, Action<Error> reportError)
        {
            request = request ?? throw new ArgumentNullException(nameof(request));

            var uri = GetRequestUri(isProduction);

            return await PostAsync<UpsAddressValidationRequest, UpsRatingResponse>(httpClient, token, uri, request, reportError);
        }
    }
}
