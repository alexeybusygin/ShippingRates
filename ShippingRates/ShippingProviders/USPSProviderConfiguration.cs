using System;
using System.Collections.Generic;
using System.Text;

namespace ShippingRates.ShippingProviders
{
    public class USPSProviderConfiguration
    {
        public string UserId { get; }
        public string Service { get; set; }
        public USPS.SpecialServices[] SpecialServices { get; set; }

        public USPSProviderConfiguration(string userId)
        {
            UserId = userId;
        }
    }
}
