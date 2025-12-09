using ShippingRates.Services.OAuth;

namespace ShippingRates.Tests.Services.OAuth;

[TestFixture()]
public class TokenCacheServiceTests
{
    [Test()]
    public void Cache_For_2_Seconds()
    {
        var clientId = "123";
        var clientId2 = "456";
        var tokenA = "aaaa";
        var tokenB = "bbbb";

        TokenCacheService.AddToken(clientId, tokenA, TimeSpan.FromSeconds(5));
        TokenCacheService.AddToken(clientId, tokenB, TimeSpan.FromSeconds(2));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(TokenCacheService.TryGetToken(clientId, out var token1), Is.True);
            Assert.That(token1, Is.EqualTo(tokenB));
        }

        Thread.Sleep(1000);

        TokenCacheService.AddToken(clientId2, tokenA, TimeSpan.FromSeconds(5));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(TokenCacheService.TryGetToken(clientId, out var token2), Is.True);
            Assert.That(token2, Is.EqualTo(tokenB));
        }

        Thread.Sleep(2000);

        Assert.That(TokenCacheService.TryGetToken(clientId, out var _), Is.False);
    }
}
