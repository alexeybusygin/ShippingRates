using ShippingRates.ShippingProviders.Usps;
using System.Diagnostics;

namespace ShippingRates.IntegrationTests.ShippingProviders.Usps;

[TestFixture]
public class UspsProviderTests : UspsProviderTestsBase
{
    private Address DomesticAddress1;
    private Address DomesticAddress2;
    private Address DomesticWrongAddress;
    private Package Package1;
    private Package Package2;
    private Package Package1SignatureRequired;
    private Package Package1WithInsurance;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        ConfigSetUp();

        DomesticAddress1 = new Address("278 Buckley Jones Road", "", "", "Cleveland", "MS", "38732", "US");
        DomesticAddress2 = new Address("One Microsoft Way", "", "", "Redmond", "WA", "98052", "US");
        DomesticWrongAddress = new Address("", "", "000", "US");

        Package1 = new Package(4, 4, 4, 5, 0);
        Package1SignatureRequired = new Package(4, 4, 4, 5, 0, signatureRequiredOnDelivery: true);
        Package1WithInsurance = new Package(4, 4, 4, 5, 50);
        Package2 = new Package(4, 4, 4, 6, 0);
    }

    [Test]
    public void GetRates_MultipleRates_ForAllServices()
    {
        var rateManager = GetRateManagerWithUspsProvider();

        var response = rateManager.GetRates(DomesticAddress1, DomesticAddress2, Package1);

        Assert.That(response, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Rates, Is.Not.Empty);
            Assert.That(response.Errors, Is.Empty);
            Assert.That(response.InternalErrors, Is.Empty);
        }

        foreach (var rate in response.Rates)
        {
            Assert.That(rate, Is.Not.Null);
            Assert.That(rate.TotalCharges, Is.GreaterThan(0));

            Debug.WriteLine($"{rate.ProviderCode} {rate.Name}: {rate.TotalCharges}");
        }
    }

    [Test]
    public void GetRates_MultipleRates_ForAllServicesAndMultiplePackages()
    {
        var rateManager = GetRateManagerWithUspsProvider();

        var response = rateManager.GetRates(DomesticAddress1, DomesticAddress2, [Package1, Package2]);

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
    public void GetRates_Error_ForInvalidAddresses()
    {
        var rateManager = GetRateManagerWithUspsProvider();

        var response = rateManager.GetRates(DomesticAddress1, DomesticWrongAddress, Package1);

        Assert.That(response, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Rates, Is.Empty);
            Assert.That(response.Errors, Is.Not.Empty);
        }
    }

    [Test]
    public void GetRates_SingleServiceResults_ForSingleServiceRequest()
    {
        var rateManager = GetRateManagerWithUspsProvider(GetConfiguration([UspsMailClass.PriorityMail]));

        var response = rateManager.GetRates(DomesticAddress1, DomesticAddress2, Package1);

        Assert.That(response, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Rates, Is.Not.Empty);
            Assert.That(response.Errors, Is.Empty);
        }
        Assert.That(response.Rates.All(r => r.Name.StartsWith("Priority Mail")), Is.True);

        Debug.WriteLine(response.Rates.First().Name + ": " + response.Rates.First().TotalCharges);
    }

    [Test]
    public void GetRates_DifferentRates_ForSignatureRequiredLookup()
    {
        var rateManager = GetRateManagerWithUspsProvider();

        var nonSignatureResponse = rateManager.GetRates(DomesticAddress1, DomesticAddress2, Package1);
        var signatureResponse = rateManager.GetRates(DomesticAddress1, DomesticAddress2, Package1SignatureRequired);

        // Assert that we have a non-signature response
        AssertIsValidNonEmptyResponse(nonSignatureResponse);

        // Assert that we have a signature response
        AssertIsValidNonEmptyResponse(signatureResponse);

        // Now compare prices
        AssertRatesAreDifferent(signatureResponse.Rates, nonSignatureResponse.Rates);
    }

    [Test]
    public void GetRates_DifferentRates_ForInsuranceLookup()
    {
        var rateManager = GetRateManagerWithUspsProvider(GetConfiguration([UspsMailClass.LibraryMail]));

        var insuranceResponse = rateManager.GetRates(DomesticAddress1, DomesticAddress2, Package1WithInsurance);
        var nonInsuranceResponse = rateManager.GetRates(DomesticAddress1, DomesticAddress2, Package1);

        AssertIsValidNonEmptyResponse(insuranceResponse);
        AssertIsValidNonEmptyResponse(nonInsuranceResponse);

        AssertRatesAreDifferent(insuranceResponse.Rates, nonInsuranceResponse.Rates);
    }

    [Test]
    public void GetRates_DifferentRates_ForSpecialServicesLookup()
    {
        var rateManager1 = GetRateManagerWithUspsProvider(GetConfiguration([UspsMailClass.LibraryMail]));

        var configuration = GetConfiguration([UspsMailClass.LibraryMail]);
        configuration.ExtraServiceCodes = [UspsExtraServiceCode.RegisteredMail];
        var rateManager2 = GetRateManagerWithUspsProvider(configuration);

        var noSpecialServicesResponse = rateManager1.GetRates(DomesticAddress1, DomesticAddress2, Package1);
        var specialServicesResponse = rateManager2.GetRates(DomesticAddress1, DomesticAddress2, Package1);

        AssertIsValidNonEmptyResponse(noSpecialServicesResponse);
        AssertIsValidNonEmptyResponse(specialServicesResponse);

        AssertRatesAreDifferent(specialServicesResponse.Rates, noSpecialServicesResponse.Rates);
    }

    [Test]
    public void GetRates_DifferentRates_ForPriceTypes()
    {
        var configuration = GetConfiguration();
        configuration.PriceType = UspsPriceType.RETAIL;
        var rateManager1 = GetRateManagerWithUspsProvider(configuration);

        var rateManager2 = GetRateManagerWithUspsProvider();

        var rates = rateManager1.GetRates(DomesticAddress1, DomesticAddress2, Package1);
        var discountedRates = rateManager2.GetRates(DomesticAddress1, DomesticAddress2, Package1);

        AssertIsValidNonEmptyResponse(rates);
        AssertIsValidNonEmptyResponse(discountedRates);

        AssertRatesAreDifferent(rates.Rates, discountedRates.Rates);
    }

    [Test, Ignore("Needs request to /shipments/v3/options/search")]
    public async Task GetRates_SaturdayDelivery()
    {
        var rateManager = GetRateManagerWithUspsProvider();

        var today = DateTime.Now;
        var nextFriday = today.AddDays(12 - (int)today.DayOfWeek).Date + new TimeSpan(10, 0, 0);
        var nextThursday = nextFriday.AddDays(-1);

        var origin = new Address("", "", "06405", "US");
        var destination = new Address("", "", "20852", "US");

        var response = await rateManager.GetRatesAsync(origin, destination, Package1, new ShipmentOptions()
        {
            ShippingDate = nextFriday,
            SaturdayDelivery = true
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.Rates, Is.Not.Empty);

        // Sometimes only Priority Mail Express 2-Day works and we have to try it on Thursday
        if (!response.Rates.Any(r => r.Options.SaturdayDelivery))
        {
            response = await rateManager.GetRatesAsync(origin, destination, Package1, new ShipmentOptions()
            {
                ShippingDate = nextThursday,
                SaturdayDelivery = true
            });

            Assert.That(response, Is.Not.Null);
            Assert.That(response.Rates, Is.Not.Empty);
        }

        Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Count));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Errors, Is.Empty);
            Assert.That(response.Rates, Has.Some.Matches<Rate>(r => r.Options.SaturdayDelivery));
        }

        foreach (var rate in response.Rates)
        {
            Assert.That(rate, Is.Not.Null);
            Assert.That(rate.TotalCharges, Is.GreaterThan(0));

            Debug.WriteLine(rate.Name + ": " + rate.TotalCharges);
        }
    }

    [Test, Ignore("Needs request to /prices/v3/letter-rates/search endpoint")]
    public async Task GetRates_FirstClassMailLetter()
    {
        var rateManager = GetRateManagerWithUspsProvider();

        const decimal firstClassLetterMaxWeight = 0.21875m; // 3.5 ounces
        var firstClassLetter = new DocumentsPackage(firstClassLetterMaxWeight, 0);

        var origin = new Address("", "", "06405", "US");
        var destination = new Address("", "", "20852", "US");

        var response = await rateManager.GetRatesAsync(origin, destination, firstClassLetter);

        Assert.That(response.Rates, Has.Some.Matches<Rate>(r =>
            r.ProviderCode is "First-Class Mail Stamped Letter" or "First-Class Mail Metered Letter"));
    }

    private static void AssertIsValidNonEmptyResponse(Shipment shipment)
    {
        Assert.That(shipment, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(shipment.Rates, Is.Not.Empty);
            Assert.That(shipment.Errors, Is.Empty);
        }
        Assert.That(shipment.Rates.First().TotalCharges, Is.GreaterThan(0));
    }

    private static void AssertRatesAreDifferent(List<Rate> ratesA, List<Rate> ratesB)
    {
        var hasDifference = false;
        foreach (var rateA in ratesA)
        {
            var rateB = ratesB.FirstOrDefault(x => x.Name == rateA.Name);
            if (rateB != null)
            {
                hasDifference |= rateA.TotalCharges != rateB.TotalCharges;
            }
            if (hasDifference)
                break;
        }

        Assert.That(hasDifference, Is.True);
    }
}
