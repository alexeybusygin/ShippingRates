using ShippingRates.ShippingProviders.FedEx;

namespace ShippingRates.IntegrationTests.ShippingProviders.FedEx;

public abstract class FedExRestSmartPostShipRatesTestsBase
{
    protected readonly RateManager _rateManager;
    protected readonly FedExSmartPostProvider _provider;

    protected FedExRestSmartPostShipRatesTestsBase()
    {
        var config = ConfigHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);

        _provider = new FedExSmartPostProvider(new FedExProviderConfiguration()
        {
            ClientId = config.FedExClientId,
            ClientSecret = config.FedExClientSecret,
            AccountNumber = config.FedExAccountNumber,
            UseProduction = config.FedExUseProduction
        });

        _rateManager = new RateManager();
        _rateManager.AddProvider(_provider);
    }
}

[TestFixture]
public class FedExSmartPostProviderTests : FedExRestSmartPostShipRatesTestsBase
{
    //[Test]
    //public void FedExSmartPostReturnsRates()
    //{
    //    var from = new Address("Annapolis", "MD", "21401", "US");
    //    var to = new Address("Fitchburg", "WI", "53711", "US");
    //    var package = new Package(7, 7, 7, 6, 0);

    //    var r = _rateManager.GetRates(from, to, package);
    //    var fedExRates = r.Rates.ToList();

    //    using (Assert.EnterMultipleScope())
    //    {
    //        Assert.That(r, Is.Not.Null);
    //        Assert.That(fedExRates, Is.Not.Empty);
    //    }

    //    foreach (var rate in fedExRates)
    //    {
    //        using (Assert.EnterMultipleScope())
    //        {
    //            Assert.That(rate.TotalCharges, Is.GreaterThan(0));
    //            Assert.That(rate.ProviderCode, Is.EqualTo("SMART_POST"));
    //        }
    //    }
    //}

    [Test]
    public void CanGetFedExServiceCodes()
    {
        var serviceCodes = _provider.GetServiceCodes();

        Assert.That(serviceCodes, Is.Not.Null);
        Assert.That(serviceCodes, Is.Not.Empty);
    }
}
