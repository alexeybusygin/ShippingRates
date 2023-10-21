using System;
using System.Collections.Generic;
using System.Text;

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
        public string PreferredCurrencyCode { get; set; }
        /// <summary>
        /// Use FedEx One Rate pricing option. Ignored for non-FedEx providers
        /// </summary>
        public bool FedExOneRate { get; set; }
        /// <summary>
        /// For FedEx One Rate pricing option, allows ability to specify FedEx-specific packages:
        /// FEDEX_10KG_BOX
        /// FEDEX_25KG_BOX
        /// FEDEX_BOX
        /// FEDEX_ENVELOPE
        /// FEDEX_EXTRA_LARGE_BOX
        /// FEDEX_LARGE_BOX
        /// FEDEX_MEDIUM_BOX
        /// FEDEX_PAK
        /// FEDEX_SMALL_BOX
        /// FEDEX_TUBE
        /// Ignored for non-FedEx providers.  Not applied uness FedExOneRate is true.
        /// </summary>
        public string FedExOneRatePackageOverride { get; set; }
    }
}
