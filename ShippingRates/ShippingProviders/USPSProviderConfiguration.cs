using System;
using System.Collections.Generic;
using System.Text;

namespace ShippingRates.ShippingProviders
{
    public class USPSProviderConfiguration
    {
        public string UserId { get; set; }
        /// <summary>
        /// If set to ALL, special service types will not be returned. This is a limitation of the USPS API.
        /// </summary>
        public string Service { get; set; }
        public USPS.SpecialServices[] SpecialServices { get; set; }

        public USPSProviderConfiguration(string userId)
        {
            UserId = userId;
        }
    }
}
