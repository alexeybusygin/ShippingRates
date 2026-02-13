using ShippingRates.ShippingProviders;

namespace ShippingRates.Tests.ShippingProviders;

[TestFixture]
public class DHLRates
{
    [Test]
    public void CanGetDHLServiceCodes()
    {
        var serviceCodes = DHLProvider.AvailableServices;

        Assert.That(serviceCodes, Is.Not.Null);
        Assert.That(serviceCodes, Is.Not.Empty);
    }
}
