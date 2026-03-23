namespace ShippingRates.ShippingProviders.Usps;

internal static class UspsMessages
{
    internal static class Error
    {
        internal const string DeserializationFailed = "Unable to deserialize USPS response.";
        internal const string UspsError = "USPS Error: {Message}";
        internal const string UspsErrorWithCode = "USPS Error: {Code} {Message}";
        internal const string UnknownError = "Unknown error '{Message}' while fetching USPS prices: {StatusCode} {Response}";
        internal const string ErrorOriginNotUS = "USPS supports only shipments with an origin address in the United States.";
    }
}
