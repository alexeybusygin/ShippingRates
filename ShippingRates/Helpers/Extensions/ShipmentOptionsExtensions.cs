using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ShippingRates.Helpers.Extensions
{
    public static class ShipmentOptionsExtensions
    {
        public static string GetCurrencyCode(this ShipmentOptions options)
        {
            return !string.IsNullOrEmpty(options?.PreferredCurrencyCode)
                ? options.PreferredCurrencyCode
                : ShipmentOptions.DefaultCurrencyCode;
        }
    }
}
