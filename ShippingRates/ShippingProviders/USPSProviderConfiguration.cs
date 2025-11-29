using ShippingRates.Models.OAuth;

namespace ShippingRates.ShippingProviders
{
    public class USPSProviderConfiguration : IOAuthConfiguration
    {
        public string UserId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public bool UseProduction { get; set; }

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
