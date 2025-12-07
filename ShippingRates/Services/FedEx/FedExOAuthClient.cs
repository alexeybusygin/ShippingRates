using Microsoft.Extensions.Logging;
using ShippingRates.Models;
using ShippingRates.Services.OAuth;
using ShippingRates.ShippingProviders;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;

namespace ShippingRates.Services.FedEx;

internal class FedExOAuthClient(ILogger logger) : OAuthClientBase<FedExProviderConfiguration>(logger)
{
    protected override string GetOAuthRequestUri(bool isProduction)
        => $"https://{(isProduction ? "apis" : "apis-sandbox")}.fedex.com/oauth/token";

    protected override string ServiceName => "FedEx";

    protected override void SetPayload(HttpRequestMessage requestMessage, FedExProviderConfiguration configuration)
    {
        var postData = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("client_id", configuration.ClientId),
            new("client_secret", configuration.ClientSecret)
        };
        requestMessage.Content = new FormUrlEncodedContent(postData);
    }

    protected override bool ParseError(string response, RateResultAggregator resultAggregator)
    {
        var errorResponse = JsonSerializer.Deserialize<Models.FedEx.FedExErrorResponse>(response);
        if (errorResponse == null
            || errorResponse.Errors == null
            || errorResponse.Errors.Length == 0)
            return false;

        foreach (var error in errorResponse.Errors)
        {
            resultAggregator.AddProviderError(new Error()
            {
                Number = error.Code,
                Description = error.Message
            });
            _logger?.LogError(OAuthMessages.Error.TokenErrorWithCode, ServiceName, error.Code, error.Message);
        }
        return true;
    }
}

