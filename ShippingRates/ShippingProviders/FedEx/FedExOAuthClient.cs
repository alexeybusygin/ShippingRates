using Microsoft.Extensions.Logging;
using ShippingRates.Services.OAuth;
using System.Collections.Generic;
using System.Net.Http;

namespace ShippingRates.ShippingProviders.FedEx
{
    internal class FedExOAuthClient : OAuthClientBase<FedExProviderConfiguration>
    {
        public FedExOAuthClient(ILogger logger)
            : base(logger)
        {
        }

        protected override string GetOAuthRequestUri(bool isProduction)
            => $"https://{(isProduction ? "apis" : "apis-sandbox")}.fedex.com/oauth/token";

        protected override string ServiceName => "FedEx";

        protected override void SetPayload(HttpRequestMessage requestMessage, FedExProviderConfiguration configuration)
        {
            var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", configuration.ClientId),
                new KeyValuePair<string, string>("client_secret", configuration.ClientSecret)
            };
            requestMessage.Content = new FormUrlEncodedContent(postData);
        }
    }
}
