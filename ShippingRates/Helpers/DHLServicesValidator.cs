using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShippingRates.ShippingProviders;

namespace ShippingRates.Helpers
{
    public static class DHLServicesValidator
    {
        public static bool IsServiceValid(char c) =>
            DHLProvider.AvailableServices.ContainsKey(char.ToUpperInvariant(c));

        public static char[] GetValidServices(char[] services)
        {
            return (services ?? Array.Empty<char>())
                .Select(c => char.ToUpperInvariant(c))
                .Where(c => DHLProvider.AvailableServices.ContainsKey(c)).ToArray();
        }
    }
}
