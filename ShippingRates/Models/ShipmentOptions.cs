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
        /// Use FedEx One Rate pricing option. Ignored for non-FedEx prodivers
        /// </summary>
        public bool FedExOneRate { get; set; }
    }
}
