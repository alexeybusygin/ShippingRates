using ShippingRates.ShippingProviders.FedEx;
using System.Net;

namespace ShippingRates.IntegrationTests.ShippingProviders.FedEx;

public abstract class FedExTestsBase
{
    protected readonly RateManager _rateManager;
    protected readonly RateManager _rateManagerNegotiated;
    protected readonly FedExProvider _provider;
    protected readonly FedExProvider _providerNegotiated;

    protected FedExTestsBase(HttpClient httpClient)
    {
        var config = ConfigHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);

        _provider = new FedExProvider(new FedExProviderConfiguration()
        {
            ClientId = config.FedExClientId,
            ClientSecret = config.FedExClientSecret,
            AccountNumber = config.FedExAccountNumber,
            UseProduction = config.FedExUseProduction
        }, httpClient);

        _rateManager = new RateManager();
        _rateManager.AddProvider(_provider);

        _providerNegotiated = new FedExProvider(new FedExProviderConfiguration()
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
public class FedExTests : FedExTestsBase
{
    public FedExTests() : base(new HttpClient(new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
    })) {
    }

    [Test]
    public void FedExReturnsRates()
    {
        var from = new Address("Annapolis", "MD", "21401", "US");
        var to = new Address("Fitchburg", "WI", "53711", "US");
        var package = new Package(7, 7, 7, 6, 0);

        var r = _rateManager.GetRates(from, to, package);
        PrintErrorIfAny(r);

        var fedExRates = r.Rates.ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(r, Is.Not.Null);
            Assert.That(fedExRates, Is.Not.Empty);
        }

        foreach (var rate in fedExRates)
        {
            Assert.That(rate.TotalCharges, Is.GreaterThan(0));
        }
    }

    [Test]
    public void FedExReturnsErrors()
    {
        var from = new Address("Annapolis", "MD", "21401", "US");
        var to = new Address("", "", "30404", "US");
        var package = new Package(7, 7, 7, 6, 1);

        var rates = _rateManager.GetRates(from, to, package);
        PrintErrorIfAny(rates);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rates, Is.Not.Null);
            Assert.That(rates.Rates, Is.Empty);
        }
        Assert.That(rates.Errors, Has.Count.EqualTo(1));

        var error = rates.Errors.FirstOrDefault();
        Assert.That(error, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(error.Number, Is.EqualTo("400"));
            Assert.That(error.Description, Is.Not.Null);
        }
        Assert.That(error.Description, Is.EqualTo("RATE.LOCATION.NOSERVICE"));
    }

    /// <summary>
    /// Note, Test and some production accounts may not have different rates with FedEx
    /// </summary>
    [Test]
    public void FedExNegotiatedRates()
    {
        var from = new Address("Annapolis", "MD", "21401", "US");
        var to = new Address("Fitchburg", "WI", "53711", "US");
        var package = new Package(7, 7, 7, 6, 1);

        var r = _rateManager.GetRates(from, to, package);
        PrintErrorIfAny(r);

        var rN = _rateManagerNegotiated.GetRates(from, to, package);
        PrintErrorIfAny(rN);

        AssertRatesAreNotEqual(r, rN, "FEDEX_GROUND");
    }

    [Test]
    public void FedExOneRate()
    {
        var from = new Address("Annapolis", "MD", "21401", "US");
        var to = new Address("Fitchburg", "WI", "53711", "US");
        var package = new Package(3, 3, 3, 6, 1);


        var rates = _rateManager.GetRates(from, to, package);
        PrintErrorIfAny(rates);

        var oneRates = _rateManagerNegotiated.GetRates(from, to, package, new ShipmentOptions()
        {
            FedExOneRate = true
        });
        PrintErrorIfAny(oneRates);

        AssertRatesAreNotEqual(rates, oneRates);
    }

    [Test]
    public void FedExOneRatePackage()
    {
        var from = new Address("Annapolis", "MD", "21401", "US");
        var to = new Address("Fitchburg", "WI", "53711", "US");
        var package = new Package(7, 7, 7, 6, 1);

        var rates = _rateManagerNegotiated.GetRates(from, to, package, new ShipmentOptions()
        {
            FedExOneRate = true,
            //if not set, will default to FEDEX_MEDIUM_BOX
        });
        PrintErrorIfAny(rates);

        var oneRates = _rateManagerNegotiated.GetRates(from, to, package, new ShipmentOptions()
        {
            FedExOneRate = true,
            FedExPackagingTypeOverride = FedExPackagingType.FedExEnvelope // one of the cheapest options
        });
        PrintErrorIfAny(oneRates);

        AssertRatesAreNotEqual(rates, oneRates);
    }

    private static void AssertRatesAreNotEqual(Shipment r1, Shipment r2, string methodCode = null)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(r1?.Rates, Is.Not.Null);
            Assert.That(r2?.Rates, Is.Not.Null);
        }
        using (Assert.EnterMultipleScope())
        {
            Assert.That(r1.Rates, Is.Not.Empty);
            Assert.That(r2.Rates, Is.Not.Empty);
        }

        var commonCode = methodCode ?? r1.Rates
            .Select(r => r.ProviderCode)
            .Where(c => r2.Rates.Select(r => r.ProviderCode).Contains(c))
            .FirstOrDefault();
        Assert.That(commonCode, Is.Not.Null);

        var rate1 = r1.Rates.FirstOrDefault(r => r.ProviderCode == commonCode);
        var rate2 = r2.Rates.FirstOrDefault(r => r.ProviderCode == commonCode);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(rate1, Is.Not.Null);
            Assert.That(rate2, Is.Not.Null);
        }
        Assert.That(rate2.TotalCharges, Is.Not.EqualTo(rate1.TotalCharges));
    }

    [Test]
    public async Task FedExSaturdayDelivery()
    {
        var from = new Address("Annapolis", "MD", "21401", "US");
        var to = new Address("Fitchburg", "WI", "53711", "US");
        var package = new Package(7, 7, 7, 6, 0);

        var today = DateTime.Now;
        var nextFriday = today.AddDays(12 - (int)today.DayOfWeek);

        var r = await _rateManager.GetRatesAsync(from, to, package, new ShipmentOptions()
        {
            ShippingDate = nextFriday,
            SaturdayDelivery = true
        });

        PrintErrorIfAny(r);
        Assert.That(r, Is.Not.Null);
        Assert.That(r.Rates, Is.Not.Empty);
        Assert.That(r.Rates.Any(r => r.Options.SaturdayDelivery), Is.True);
    }

    [Test]
    public async Task FedExCurrency()
    {
        var from = new Address("", "", "220-8515", "JP");
        var to = new Address("", "", "058357", "SG");
        var package = new Package(1, 1, 1, 5, 1);

        var r = await _rateManager.GetRatesAsync(from, to, package);

        PrintErrorIfAny(r);
        Assert.That(r, Is.Not.Null);
        Assert.That(r.Rates, Is.Not.Empty);
        Assert.That(r.Rates.Any(r => r.TotalCharges > 1000), Is.False);
    }

    [Test]
    public async Task FedExPreferredCurrency()
    {
        var from = new Address("", "", "220-8515", "JP");
        var to = new Address("", "", "058357", "SG");
        var package = new Package(1, 1, 1, 5, 1);

        var r = await _rateManager.GetRatesAsync(from, to, package, new ShipmentOptions()
        {
            PreferredCurrencyCode = "USD"
        });

        PrintErrorIfAny(r);
        Assert.That(r, Is.Not.Null);
        Assert.That(r.Rates, Is.Not.Empty);
        Assert.That(r.Rates.Any(r => r.CurrencyCode != "USD"), Is.False);

        var rEuro = await _rateManager.GetRatesAsync(from, to, package, new ShipmentOptions()
        {
            PreferredCurrencyCode = "EUR"
        });

        PrintErrorIfAny(rEuro);
        Assert.That(rEuro, Is.Not.Null);
        Assert.That(rEuro.Rates, Is.Not.Empty);
        Assert.That(rEuro.Rates.Any(r => r.CurrencyCode != "EUR"), Is.False);
    }

    private static void PrintErrorIfAny(Shipment result)
    {
        if(result.Errors.Count != 0)
        {
            Console.WriteLine("Errors:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  {error.Number}: {error.Description}");
            }
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
