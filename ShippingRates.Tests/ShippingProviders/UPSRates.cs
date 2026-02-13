using ShippingRates.ShippingProviders;

namespace ShippingRates.Tests.ShippingProviders;

[TestFixture]
public class UPSRates
{
    [Test]
    public void CanGetUpsServiceCodes()
    {
        var serviceCodes = UPSProvider.GetServiceCodes();

        Assert.That(serviceCodes, Is.Not.Null);
        Assert.That(serviceCodes, Is.Not.Empty);
    }
}
