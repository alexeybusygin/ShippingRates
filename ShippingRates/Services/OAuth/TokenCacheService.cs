using System;
using System.Collections.Concurrent;

namespace ShippingRates.Services.OAuth;

/// <summary>
/// Simple in-memory cache for UPS, FedEx, and USPS OAuth tokens.
/// </summary>
internal static class TokenCacheService
{
    static readonly ConcurrentDictionary<string, CacheItem> _cache = new();
    /// <summary>
    /// Try to get a cached, unexpired token for the client.
    /// </summary>
    /// <param name="clientId">Unique identifier for the client.</param>
    /// <returns><c>true</c> when a valid token is returned.</returns>
    public static bool TryGetToken(string clientId, out string? token)
    {
        var now = DateTimeOffset.UtcNow;
        if (_cache.TryGetValue(clientId, out var item) && item.ExpirationTime > now)
        {
            token = item.Token;
            return true;
        }

        token = null;
        return false;
    }

    /// <summary>
    /// Store or refresh the token for the client.
    /// </summary>
    /// <param name="clientId">Unique identifier for the client.</param>
    /// <param name="token">OAuth access token.</param>
    /// <param name="expiresIn">Time-to-live before the token expires.</param>
    public static void AddToken(string clientId, string token, TimeSpan expiresIn)
    {
        if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentException("clientId required", nameof(clientId));
        if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("token required", nameof(token));

        var now = DateTimeOffset.UtcNow;
        var item = new CacheItem(token, now + expiresIn);

        _cache.AddOrUpdate(clientId, item, (_, _) => item);
    }

    private sealed record CacheItem(string Token, DateTimeOffset ExpirationTime);
}
