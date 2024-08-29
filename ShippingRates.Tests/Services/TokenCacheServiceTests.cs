using NUnit.Framework;
using ShippingRates.Services;
using System.Threading;

namespace ShippingRates.Tests.Services
{
    [TestFixture()]
    public class TokenCacheServiceTests
    {
        [Test()]
        public void Cache_For_2_Seconds()
        {
            var clientId = "123";
            var clientId2 = "1234";
            var tokenA = "aaaa";
            var tokenB = "bbbb";

            TokenCacheService.AddToken(clientId, tokenA, 5);
            TokenCacheService.AddToken(clientId, tokenB, 2);

            Assert.That(TokenCacheService.GetToken(clientId), Is.EqualTo(tokenB));

            Thread.Sleep(1000);

            TokenCacheService.AddToken(clientId2, tokenA, 5);

            Assert.That(TokenCacheService.GetToken(clientId), Is.EqualTo(tokenB));

            Thread.Sleep(2000);

            Assert.That(TokenCacheService.GetToken(clientId), Is.Null);
        }
    }
}
