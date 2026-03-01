using ShippingRates.ShippingProviders.FedEx;
using System.Net;

namespace ShippingRates.IntegrationTests.ShippingProviders.FedEx;

public abstract class FedExFrieghtLtProviderTestsBase
{
    protected readonly RateManager _rateManager;
    protected readonly RateManager _rateManagerNegotiated;
    protected readonly FedExFreightLtlProvider _provider;
    protected readonly FedExFreightLtlProvider _providerNegotiated;

    protected FedExFrieghtLtProviderTestsBase(HttpClient httpClient)
    {
        var config = ConfigHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);

        _provider = new FedExFreightLtlProvider(new FedExProviderConfiguration()
        {
            ClientId = config.FedExClientId,
            ClientSecret = config.FedExClientSecret,
            AccountNumber = config.FedExAccountNumber,
            UseProduction = config.FedExUseProduction
        }, httpClient);

        _rateManager = new RateManager();
        _rateManager.AddProvider(_provider);

        _providerNegotiated = new FedExFreightLtlProvider(new FedExProviderConfiguration()
        {
            ClientId = config.FedExClientId,
            ClientSecret = config.FedExClientSecret,
            AccountNumber = config.FedExAccountNumber,
            UseProduction = config.FedExUseProduction,
            UseNegotiatedRates = true
        }, httpClient);

        _rateManagerNegotiated = new RateManager();
        _rateManagerNegotiated.AddProvider(_providerNegotiated);
    }
}

[TestFixture]
public class FedExFrieghtLtProviderTests : FedExFrieghtLtProviderTestsBase
{
    public FedExFrieghtLtProviderTests() : base(new HttpClient(new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
    }))
    {
    }

    // Require proper freight account setup in FedEx account
    [Test]
    public void FedExFreight()
    {
        var from = new Address("Annapolis", "MD", "21401", "US");
        var to = new Address("Fitchburg", "WI", "53711", "US");
        var package = new Package(48, 48, 48, 120, 100, "CONTAINER");

        var rates = _rateManager.GetRates(from, to, package);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rates.Rates, Is.Not.Empty);
            Assert.That(rates.Rates.All(r => r.Name.Contains("Freight")), Is.True);
        }
    }

    [Test]
    public void CanGetFedExServiceCodes()
    {
        var serviceCodes = _provider.GetServiceCodes();

        Assert.That(serviceCodes, Is.Not.Null);
        Assert.That(serviceCodes, Is.Not.Empty);
    }
}
