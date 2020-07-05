using System;
using System.Collections.Generic;
using System.Text;

namespace ShippingRates
{
    public class ShipmentOptions
    {
        public bool SaturdayDelivery { get; set; } = false;
        public DateTime? ShippingDate { get; set; } = null;
    }
}
