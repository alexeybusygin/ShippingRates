using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ShippingRates.ShippingProviders
{
    public class USPSBaseProvider : AbstractShippingProvider
    {
        protected void ParseErrors(XElement response)
        {
            if (response.Descendants("Error").Any())
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
