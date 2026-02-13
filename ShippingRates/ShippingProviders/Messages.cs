using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingRates.ShippingProviders
{
    internal static class Messages
    {
        internal static class Information
        {
            public const string DeserializationFailed = "Unable to deserialize response.";
            public const string UnknownError = "Unknown error '{Message}' while fetching prices: {StatusCode} {Response}";
        }
    }
}
