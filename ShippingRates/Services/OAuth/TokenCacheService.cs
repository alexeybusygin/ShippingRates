using System;
using System.Collections.Concurrent;

namespace ShippingRates.Services.OAuth
{
    /// <summary>
    /// Token caching for UPS, FedEx, and USPS OAuth tokens
    /// </summary>
    internal class TokenCacheService
    {
        static readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();
        /// <summary>
        /// Get token for a given client ID
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <returns>Token string or null</returns>
        public static string GetToken(string clientId)
        {
            if (_cache.TryGetValue(clientId, out var item))
            {
                if (item.ExpirationTime > DateTime.Now)
                {
                    return item.Token;
                }
            }
            return null;
        }

        /// <summary>
        /// Add token
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="token">Token</param>
        /// <param name="expiresIn">Expiration interval in seconds</param>
        public static void AddToken(string clientId, string token, int expiresIn)
        {
            var item = new CacheItem()
            {
                Token = token,
                ExpirationTime = DateTime.Now.AddSeconds(expiresIn)
            };
            _cache.AddOrUpdate(clientId, item, (key, existingVal) => item);
        }

        class CacheItem
        {
            public string Token { get; set; }
            public DateTime ExpirationTime { get; set; }
        }
    }
}
