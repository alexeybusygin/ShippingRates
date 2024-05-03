using System;
using System.Collections.Generic;
using System.Text;

namespace ShippingRates.ShippingProviders
{
    public class UPSProviderConfiguration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AccountNumber { get; set; }
    }
}
