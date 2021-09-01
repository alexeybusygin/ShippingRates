using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ShippingRates.ShippingProviders
{
    public abstract class USPSBaseProvider : AbstractShippingProvider
    {
        public override string Name { get => "USPS"; }

        protected const string USPSCurrencyCode = "USD";
        protected const string ProductionUrl = "https://secure.shippingapis.com/ShippingAPI.dll";

        protected void ParseErrors(XElement response)
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
                    AddError(err);
                }
            }
        }
    }
}
