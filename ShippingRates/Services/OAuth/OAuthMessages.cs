namespace ShippingRates.Services.OAuth
{
    internal class OAuthMessages
    {
        internal static class Debug
        {
            public const string TokenFetchedFromCache = "Fetched {ServiceName} token from cache for clientId {ClientId}";
            public const string TokenReceived = "Received OAuth token from {ServiceName}";
        }

        internal static class Error
        {
            public const string TokenErrorWithCode = "Error while fetching {ServiceName} OAuth token: {Code} {Message}";
            public const string TokenError = "Error while fetching {ServiceName} OAuth token: {Message}";
            public const string Unknown = "Unknown error while fetching {ServiceName} OAuth token: {StatusCode} {Response}";
        }
    }
}
