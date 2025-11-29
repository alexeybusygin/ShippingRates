using Microsoft.Extensions.Logging;
using ShippingRates.Models;
using ShippingRates.Models.Usps;
using ShippingRates.Services.OAuth;
using System.Text.Json;

namespace ShippingRates.ShippingProviders.Usps
{
    internal class UspsOAuthClient : OAuthClientBase<USPSProviderConfiguration>
    {
        public UspsOAuthClient(ILogger logger)
            : base(logger)
        {
        }

        protected override string GetOAuthRequestUri(bool isProduction)
            => $"https://{(isProduction ? "apis" : "apis-tem")}.usps.com/oauth2/v3/token";

        protected override string ServiceName => "USPS";

        protected override bool ParseError(string response, RateResultAggregator resultAggregator)
        {
            var error = JsonSerializer.Deserialize<UspsErrorResponse>(response);
            if (error == null)
                return false;

            resultAggregator.AddProviderError(new Error()
            {
                Description = error.ErrorDescription,
            });
            _logger?.LogError("Error while fetching {ServiceName} OAuth token: {ErrorDescription}", ServiceName, error.ErrorDescription);

            return true;
        }
    }
}
