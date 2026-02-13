using ShippingRates.ShippingProviders.Usps;
using System.Diagnostics;

namespace ShippingRates.IntegrationTests.ShippingProviders.Usps;

[TestFixture]
public class UspsProviderInternationalTests : UspsProviderTestsBase
{
    private const decimal FirstClassLetterMaxWeight = 3.5m;
    private const decimal FirstClassPackageMaxWeight = 4.0m;

    private Address _domesticAddress;
    private Address _internationalAddress;
    private Address _internationalAddressWrong;
    private DocumentsPackage _firstClassLetterWithNoValue;
    private DocumentsPackage _firstClassLetterWithValue;
    private Package _package1;
    private Package _package2;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        ConfigSetUp();

        _domesticAddress = new Address("278 Buckley Jones Road", "", "", "Cleveland", "MS", "38732", "US");
        _internationalAddress = new Address("80-100 Victoria St", "", "", "London", "", "SW1E 5JL", "GB");
        _internationalAddressWrong = new Address("", "", "", "", "", "12345", "XX");

        _firstClassLetterWithNoValue = new DocumentsPackage(FirstClassLetterMaxWeight, 0);
        _firstClassLetterWithValue = new DocumentsPackage(FirstClassLetterMaxWeight, 1);
        _package1 = new Package(14, 14, 14, 15, 0);
        _package2 = new Package(6, 6, 6, 5, 100);
    }


    [Test]
    public void GetRates_MultipleRatesForAllServices()
    {
        var rateManager = GetRateManagerWithUspsProvider();

        var response = rateManager.GetRates(_domesticAddress, _internationalAddress, _package1);

        Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Count));

        Assert.That(response, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Rates, Is.Not.Empty);
            Assert.That(response.Errors, Is.Empty);
        }
        Assert.That(response.Rates.Any(r => r.CurrencyCode != "USD"), Is.False);

        foreach (var rate in response.Rates)
        {
            Assert.That(rate, Is.Not.Null);
            Assert.That(rate.TotalCharges, Is.GreaterThan(0));

            Debug.WriteLine(rate.Name + ": " + rate.TotalCharges);
        }
    }

    [Test]
    public void GetRates_MultipleRatesForAllServicesAndMultiplePackages()
    {
        var configuration = GetConfiguration();
        configuration.ProcessingCategory = UspsProcessingCategory.FLATS;
        var rateManager = GetRateManagerWithUspsProvider(configuration);

        var packages = new List<Package>
        {
            _package1,
            _package2
        };
        var response = rateManager.GetRates(_domesticAddress, _internationalAddress, packages);

        Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Count));

        Assert.That(response, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Rates, Is.Not.Empty);
            Assert.That(response.Errors, Is.Empty);
        }
        Assert.That(response.Rates.Any(r => r.CurrencyCode != "USD"), Is.False);

        foreach (var rate in response.Rates)
        {
            Assert.That(rate, Is.Not.Null);
            Assert.That(rate.TotalCharges, Is.GreaterThan(0));

            Debug.WriteLine(rate.Name + ": " + rate.TotalCharges);
        }
    }


    [Test]
    public void GetRates_NoRatesForInvalidAddress()
    {
        var rateManager = GetRateManagerWithUspsProvider();

        var response = rateManager.GetRates(_domesticAddress, _internationalAddressWrong, _package1);

        Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Count));

        Assert.That(response, Is.Not.Null);
        Assert.That(response.Rates, Is.Empty);
    }

    [Test]
    public void GetRates_SingleRateForSingleService()
    {
        var rateManager = GetRateManagerWithUspsProvider(GetConfiguration(UspsMailClass.PriorityMailInternational));

        var response = rateManager.GetRates(_domesticAddress, _internationalAddress, _package1);

        Assert.That(response, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Errors, Is.Empty);
            Assert.That(response.Rates, Has.Count.EqualTo(1));
        }
        Assert.That(response.Rates.First().TotalCharges, Is.GreaterThan(0));
    }

    [Test]
    public void GetRates_FirstClassMailRatesForFirstClassLetterWithNoValue()
    {
        var rateManager = GetRateManagerWithUspsProvider();

        var response = rateManager.GetRates(_domesticAddress, _internationalAddress, _firstClassLetterWithNoValue);
        Assert.That(response.Rates.Any(IsFirstClassMailRate), Is.True);
    }

    [Test]
    public void GetRates_NoFirstClassMailRatesForFirstClassLetterWithValue()
    {
        var configuration = GetConfiguration();
        configuration.ProcessingCategory = UspsProcessingCategory.FLATS;
        var rateManager = GetRateManagerWithUspsProvider(configuration);

        var response = rateManager.GetRates(_domesticAddress, _internationalAddress, _firstClassLetterWithValue);
        Assert.That(response.Rates.Any(IsFirstClassMailRate), Is.False);
    }

    [Test]
    public void GetRates_NoFirstClassMailRatesForLargePackage()
    {
        var rateManager = GetRateManagerWithUspsProvider();

        var package = new Package(7m, 5m, 0.1m, FirstClassPackageMaxWeight + 0.01m, 0);

        var response = rateManager.GetRates(_domesticAddress, _internationalAddress, package);
        Assert.That(response.Rates.Any(IsFirstClassMailRate), Is.False);
    }

    private bool IsFirstClassMailRate(Rate rate)
    {
        return rate.ProviderCode.StartsWith("First-Class");
    }
}
