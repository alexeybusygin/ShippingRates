namespace ShippingRates.Services.OAuth;

internal static class OAuthMessages
{
    internal static class Debug
    {
        internal const string TokenFetchedFromCache = "Fetched {ServiceName} token from cache for clientId {ClientId}";
        internal const string TokenReceived = "Received OAuth token from {ServiceName}";
    }

    internal static class Error
    {
        internal const string ClientIdMissing = "ClientId is required.";
        internal const string TokenErrorWithCode = "Error while fetching {ServiceName} OAuth token. Provider returned {Code}: {Message}";
        internal const string TokenError = "Error while fetching {ServiceName} OAuth token. Provider returned: {Message}";
        internal const string Unknown = "Unexpected response fetching {ServiceName} OAuth token. Status {StatusCode}, body: {Response}";
    }
}
