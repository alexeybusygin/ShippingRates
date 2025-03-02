using ShippingRates.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;

namespace ShippingRates.ShippingProviders
{
    public abstract class USPSBaseProvider : AbstractShippingProvider
    {
        public override string Name { get => "USPS"; }

        protected const string USPSCurrencyCode = "USD";
        protected const string ProductionUrl = "https://secure.shippingapis.com/ShippingAPI.dll";

        protected readonly USPSProviderConfiguration _configuration;

        public USPSBaseProvider(USPSProviderConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if (string.IsNullOrEmpty(_configuration.Service))
            {
                _configuration.Service = USPS.Services.All;
            }
        }

        public USPSBaseProvider(USPSProviderConfiguration configuration, HttpClient httpClient)
            : this(configuration)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        internal static void ParseErrors(XElement response, RateResultBuilder resultBuilder)
        {
            if (response?.Descendants("Error")?.Any() ?? false)
            {
                var errors = response
                    .Descendants("Error")
                    .Select(item => new Error()
                    {
                        Description = item.Element("Description")?.Value?.ToString(),
                        Source = item.Element("Source")?.Value?.ToString(),
                        HelpContext = item.Element("HelpContext")?.Value?.ToString(),
                        HelpFile = item.Element("HelpFile")?.Value?.ToString(),
                        Number = item.Element("Number")?.Value?.ToString()
                    });

                foreach (var err in errors)
                {
                    resultBuilder.AddError(err);
                }
            }
        }
    }
}
