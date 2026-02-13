using ShippingRates.Models.OAuth;

namespace ShippingRates.ShippingProviders
{
    public class FedExProviderConfiguration : IOAuthConfiguration
    {
        /// <summary>
        /// FedEx Account Number
        /// </summary>
        public string? AccountNumber { get; set; }
        public string? Key { get; set; }
        public string? MeterNumber { get; set; }
        public string? Password { get; set; }
        public bool UseProduction { get; set; }
        public bool UseNegotiatedRates { get; set; } = false;
        /// <summary>
        /// Hub ID for FedEx SmartPost.
        /// If not using the production Rate API, you can use 5531 as the HubID per FedEx documentation.
        /// </summary>
        public string? HubId { get; set; }
        /// <summary>
        /// FedEx Client Id (required for REST API)
        /// </summary>
        public string? ClientId { get; set; }
        /// <summary>
        /// FedEx Client Secret (required for REST API)
        /// </summary>
        public string? ClientSecret { get; set; }
    }
}
