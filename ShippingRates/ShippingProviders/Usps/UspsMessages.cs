using System.Globalization;

namespace ShippingRates.ShippingProviders.Usps;

internal static class UspsMessages
{
    internal static class Error
    {
        public const string DeserializationFailed = "Unable to deserialize USPS response.";
        public const string UspsError = "USPS Error: {Message}";
        public const string UspsErrorWithCode = "USPS Error: {Code} {Message}";
        public const string UnknownError = "Unknown error '{Message}' while fetching USPS prices: {StatusCode} {Response}";
        public const string ErrorOriginNotUS = "USPS supports only shipments with an origin address in the United States.";
    }
}
