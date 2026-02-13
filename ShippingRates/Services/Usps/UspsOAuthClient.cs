using Microsoft.Extensions.Logging;
using ShippingRates.Models;
using ShippingRates.Models.Usps;
using ShippingRates.Services.OAuth;
using ShippingRates.ShippingProviders.Usps;
using System.Text.Json;

namespace ShippingRates.Services.Usps;

internal sealed class UspsOAuthClient(ILogger? logger) : OAuthClientBase<UspsProviderConfiguration>(logger)
{
    protected override string ServiceName => "USPS";

    protected override string GetOAuthRequestUri(bool isProduction)
        => $"https://{(isProduction ? "apis" : "apis-tem")}.usps.com/oauth2/v3/token";

    protected override bool ParseError(string response, RateResultAggregator resultAggregator)
    {
        var error = JsonSerializer.Deserialize<UspsOAuthErrorResponse>(response);
        if (error == null)
            return false;

        resultAggregator.AddProviderError(new Error()
        {
            Description = error.ErrorDescription,
        });
        Logger?.LogError(OAuthMessages.Error.TokenError, ServiceName, error.ErrorDescription);

        return true;
    }
}
