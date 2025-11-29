using ShippingRates.ShippingProviders;
using System.Diagnostics;

namespace ShippingRates.IntegrationTests.ShippingProviders.Usps;

[TestFixture, Ignore("Switching to REST")]
public class USPSInternationalProviderTests
{
    private const decimal FirstClassLetterMaxWeight = 0.21875m; // 3.5 ounces

    private Address _domesticAddress1;
    private Address _domesticAddress2;
    private Address _internationalAddress1;
    private Address _internationalAddress2;
    private DocumentsPackage _firstClassLetterWithNoValue;
    private DocumentsPackage _firstClassLetterWithValue;
    private Package _package1;
    private Package _package2;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _domesticAddress1 = new Address("278 Buckley Jones Road", "", "", "Cleveland", "MS", "38732", "US");
        _domesticAddress2 = new Address("One Microsoft Way", "", "", "Redmond", "WA", "98052", "US");
        _internationalAddress1 = new Address("Jubail", "Jubail", "31951", "SA"); //has limited international services available
        _internationalAddress2 = new Address("80-100 Victoria St", "", "", "London", "", "SW1E 5JL", "GB");

        _firstClassLetterWithNoValue = new DocumentsPackage(FirstClassLetterMaxWeight, 0);
        _firstClassLetterWithValue = new DocumentsPackage(FirstClassLetterMaxWeight, 1);
        _package1 = new Package(14, 14, 14, 15, 0);
        _package2 = new Package(6, 6, 6, 5, 100);
    }

    private static USPSProviderConfiguration GetConfiguration(string? service = null)
    {
        return new USPSProviderConfiguration(ConfigHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory).USPSUserId)
        {
            Service = service
        };
    }

    [Test]
    public void USPS_Intl_Returns_Multiple_Rates_When_Using_Multiple_Packages_For_All_Services_And_Multiple_Packages()
    {
        var packages = new List<Package>
        {
            _package1,
            _package2
        };

        using var httpClient = new HttpClient();
        var rateManager = new RateManager();
        rateManager.AddProvider(new USPSInternationalProvider(GetConfiguration(), httpClient));

        var response = rateManager.GetRates(_domesticAddress1, _internationalAddress2, packages);

        Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Count));

        Assert.That(response, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Rates, Is.Not.Empty);
            Assert.That(response.Errors, Is.Empty);
        }

        foreach (var rate in response.Rates)
        {
            Assert.That(rate, Is.Not.Null);
            Assert.That(rate.TotalCharges, Is.GreaterThan(0));

            Debug.WriteLine(rate.Name + ": " + rate.TotalCharges);
        }
    }

    [Test]
    public void USPS_Intl_Returns_Multiple_Rates_When_Using_Valid_Addresses_For_All_Services()
    {
        using var httpClient = new HttpClient();
        var rateManager = new RateManager();
        rateManager.AddProvider(new USPSInternationalProvider(GetConfiguration(), httpClient));

        var response = rateManager.GetRates(_domesticAddress1, _internationalAddress2, _package1);

        Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Count));

        Assert.That(response, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Rates, Is.Not.Empty);
            Assert.That(response.Errors, Is.Empty);
        }

        foreach (var rate in response.Rates)
        {
            Assert.That(rate, Is.Not.Null);
            Assert.That(rate.TotalCharges, Is.GreaterThan(0));

            Debug.WriteLine(rate.Name + ": " + rate.TotalCharges);
        }
    }

    [Test]
    public void USPS_Intl_Returns_No_Rates_When_Using_Invalid_Addresses_For_All_Services()
    {
        using var httpClient = new HttpClient();
        var rateManager = new RateManager();
        rateManager.AddProvider(new USPSInternationalProvider(GetConfiguration(), httpClient));

        var response = rateManager.GetRates(_domesticAddress1, _domesticAddress2, _package1);

        Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Count));

        Assert.That(response, Is.Not.Null);
        Assert.That(response.Rates, Is.Empty);
    }

    [Test]
    public void USPS_Intl_Returns_No_Rates_When_Using_Invalid_Addresses_For_Single_Service()
    {
        //can't rate international with a domestic address

        using var httpClient = new HttpClient();
        var rateManager = new RateManager();
        rateManager.AddProvider(new USPSInternationalProvider(GetConfiguration("Priority Mail International"), httpClient));

        var response = rateManager.GetRates(_domesticAddress1, _domesticAddress2, _package1);

        Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Count));

        Assert.That(response, Is.Not.Null);
        Assert.That(response.Rates, Is.Empty);
    }

    [Test]
    public void USPS_Intl_Returns_Single_Rate_When_Using_Valid_Addresses_For_Single_Service()
    {
        using var httpClient = new HttpClient();
        var rateManager = new RateManager();
        rateManager.AddProvider(new USPSInternationalProvider(GetConfiguration("Priority Mail International"), httpClient));

        var response = rateManager.GetRates(_domesticAddress1, _internationalAddress2, _package1);

        Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Count));

        Assert.That(response, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Errors, Is.Empty);
            Assert.That(response.Rates, Has.Count.EqualTo(1));
        }
        Assert.That(response.Rates.First().TotalCharges, Is.GreaterThan(0));

        Debug.WriteLine(response.Rates.First().Name + ": " + response.Rates.First().TotalCharges);
    }

    [Test]
    public void USPS_Intl_Returns_First_Class_Mail_Rates_For_FirstClassLetterWithNoValue()
    {
        using var httpClient = new HttpClient();
        var rateManager = new RateManager();
        rateManager.AddProvider(new USPSInternationalProvider(GetConfiguration(), httpClient));

        var response = rateManager.GetRates(_domesticAddress1, _internationalAddress2, _firstClassLetterWithNoValue);
        Assert.That(response.Rates.Any(IsFirstClassMailRate), Is.True);
    }

    [Test]
    public void USPS_Intl_Returns_No_First_Class_Mail_Rates_For_FirstClassLetterWithValue()
    {
        using var httpClient = new HttpClient();
        var rateManager = new RateManager();
        rateManager.AddProvider(new USPSInternationalProvider(GetConfiguration(), httpClient));

        var response = rateManager.GetRates(_domesticAddress1, _internationalAddress2, _firstClassLetterWithValue);
        Assert.That(response.Rates.Any(IsFirstClassMailRate), Is.False);
    }

    [Test]
    public void USPS_Intl_Returns_No_First_Class_Mail_Rates_For_Package()
    {
        using var httpClient = new HttpClient();
        var rateManager = new RateManager();
        rateManager.AddProvider(new USPSInternationalProvider(GetConfiguration(), httpClient));

        var package = new Package(7m, 5m, 0.1m, FirstClassLetterMaxWeight, 0);

        var response = rateManager.GetRates(_domesticAddress1, _internationalAddress2, package);
        Assert.That(response.Rates.Any(IsFirstClassMailRate), Is.False);
    }

    [Test]
    public void CanGetUspsInternationalServiceCodes()
    {
        var provider = new USPSInternationalProvider(GetConfiguration());
        var serviceCodes = provider.GetServiceCodes();

        Assert.That(serviceCodes, Is.Not.Null);
        Assert.That(serviceCodes, Is.Not.Empty);
    }

    [Test]
    public async Task USPSCurrency()
    {
        using var httpClient = new HttpClient();
        var rateManager = new RateManager();
        rateManager.AddProvider(new USPSInternationalProvider(GetConfiguration(), httpClient));

        var response = await rateManager.GetRatesAsync(_domesticAddress1, _internationalAddress2, _package1);
        var rates = response.Rates;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(rates, Is.Not.Empty);
            Assert.That(rates.Any(r => r.CurrencyCode != "USD"), Is.False);
        }
    }

    private bool IsFirstClassMailRate(Rate rate)
    {
        return rate.ProviderCode is "First-Class Mail International Letter" or
            "First-Class Mail International Large Envelope";
    }
}
