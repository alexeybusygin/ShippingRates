namespace ShippingRates.Services.OAuth;

internal static class OAuthMessages
{
    internal static class Debug
    {
        public const string TokenFetchedFromCache = "Fetched {ServiceName} token from cache for clientId {ClientId}";
        public const string TokenReceived = "Received OAuth token from {ServiceName}";
    }

    internal static class Error
    {
        public const string ClientIdMissing = "ClientId is required.";
        public const string TokenErrorWithCode = "Error while fetching {ServiceName} OAuth token. Provider returned {Code}: {Message}";
        public const string TokenError = "Error while fetching {ServiceName} OAuth token. Provider returned: {Message}";
        public const string Unknown = "Unexpected response fetching {ServiceName} OAuth token. Status {StatusCode}, body: {Response}";
    }
}
