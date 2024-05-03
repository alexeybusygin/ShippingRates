using System;
using System.Collections.Concurrent;

namespace ShippingRates.Services
{
    internal class TokenCacheService
    {
        static readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();

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
