using System;
using ShippingRates.ShippingProviders.FedEx;

namespace ShippingRates
{
    public class ShipmentOptions
    {
        public const string DefaultCurrencyCode = "USD";

        /// <summary>
        /// Enable Saturday Delivery option for shipping rates
        /// </summary>
        public bool SaturdayDelivery { get; set; }
        /// <summary>
        /// Pickup date. Current date and time is used if not specified.
        /// </summary>
        public DateTime? ShippingDate { get; set; }
        /// <summary>
        /// Preferred currency code, applies to FedEx only
        /// </summary>
        public string? PreferredCurrencyCode { get; set; }
        /// <summary>
        /// Use FedEx One Rate pricing option. Ignored for non-FedEx providers
        /// </summary>
        public bool FedExOneRate { get; set; }
        /// <summary>
        /// FedEx packaging type override for this shipment.
        /// Ignored for non-FedEx providers.
        /// </summary>
        public FedExPackagingType? FedExPackagingTypeOverride { get; set; }
        /// <summary>
        /// Legacy FedEx One Rate package override.
        /// Ignored for non-FedEx providers. Not applied unless FedExOneRate is true.
        /// Prefer <see cref="FedExPackagingTypeOverride"/>.
        /// </summary>
        [Obsolete("Use FedExPackagingTypeOverride instead.")]
        public string? FedExOneRatePackageOverride { get; set; }
    }
}
