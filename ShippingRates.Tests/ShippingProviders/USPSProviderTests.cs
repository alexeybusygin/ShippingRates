using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using ShippingRates.ShippingProviders;

namespace ShippingRates.Tests.ShippingProviders
{
    [TestFixture]
    public class USPSProviderTests
    {
        private Package Package2;
        private readonly Address DomesticAddress1;
        private readonly Address DomesticAddress2;
        private readonly Address InternationalAddress1;
        private readonly Package Package1;
        private readonly Package Package1SignatureRequired;
        private readonly string _uspsUserId;

        public USPSProviderTests()
        {
            DomesticAddress1 = new Address("278 Buckley Jones Road", "", "", "Cleveland", "MS", "38732", "US");
            DomesticAddress2 = new Address("One Microsoft Way", "", "", "Redmond", "WA", "98052", "US");
            InternationalAddress1 = new Address("Jubail", "Jubail", "31951", "Saudi Arabia");

            Package1 = new Package(4, 4, 4, 5, 0);
            Package1SignatureRequired = new Package(4, 4, 4, 5, 0, null, true);
            Package2 = new Package(6, 6, 6, 5, 100);

            _uspsUserId = ConfigHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory)
                .USPSUserId;
        }

        [Test]
        public void USPS_Domestic_Returns_Multiple_Rates_When_Using_Valid_Addresses_For_All_Services()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSProvider(_uspsUserId));

            var response = rateManager.GetRates(DomesticAddress1, DomesticAddress2, Package1);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.IsNotEmpty(response.Rates);
            Assert.IsEmpty(response.Errors);
            Assert.IsEmpty(response.InternalErrors);

            foreach (var rate in response.Rates)
            {
                Assert.NotNull(rate);
                Assert.True(rate.TotalCharges > 0);

                Debug.WriteLine(rate.Name + ": " + rate.TotalCharges);
            }
        }

        [Test]
        public void USPS_Domestic_Returns_Multiple_Rates_When_Using_Valid_Addresses_For_All_Services_And_Multiple_Packages()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSProvider(_uspsUserId));

            var response = rateManager.GetRates(DomesticAddress1, DomesticAddress2, Package1);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.IsNotEmpty(response.Rates);
            Assert.IsEmpty(response.Errors);

            foreach (var rate in response.Rates)
            {
                Assert.NotNull(rate);
                Assert.True(rate.TotalCharges > 0);

                Debug.WriteLine(rate.Name + ": " + rate.TotalCharges);
            }
        }

        [Test]
        public void USPS_Domestic_Returns_No_Rates_When_Using_Invalid_Addresses_For_All_Services()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSProvider(_uspsUserId));

            var response = rateManager.GetRates(DomesticAddress1, InternationalAddress1, Package1);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.IsEmpty(response.Rates);
            Assert.IsEmpty(response.Errors);
        }

        [Test]
        public void USPS_Domestic_Returns_No_Rates_When_Using_Invalid_Addresses_For_Single_Service()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSProvider(_uspsUserId, "Priority Mail"));

            var response = rateManager.GetRates(DomesticAddress1, InternationalAddress1, Package1);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.IsEmpty(response.Rates);
            Assert.IsEmpty(response.Errors);
        }

        [Test]
        public void USPS_Domestic_Returns_Single_Rate_When_Using_Valid_Addresses_For_Single_Service()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSProvider(_uspsUserId, "Priority Mail"));

            var response = rateManager.GetRates(DomesticAddress1, DomesticAddress2, Package1);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.IsNotEmpty(response.Rates);
            Assert.IsEmpty(response.Errors);
            Assert.AreEqual(response.Rates.Count, 1);
            Assert.True(response.Rates.First().TotalCharges > 0);

            Debug.WriteLine(response.Rates.First().Name + ": " + response.Rates.First().TotalCharges);
        }

        [Test]
        public void CanGetUspsServiceCodes()
        {
            var provider = new USPSProvider(_uspsUserId);
            var serviceCodes = provider.GetServiceCodes();

            Assert.NotNull(serviceCodes);
            Assert.IsNotEmpty(serviceCodes);
        }

        [Test]
        public void Can_Get_Different_Rates_For_Signature_Required_Lookup()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSProvider(_uspsUserId, "Priority Mail"));

            var nonSignatureResponse = rateManager.GetRates(DomesticAddress1, DomesticAddress2, Package1);
            var signatureResponse = rateManager.GetRates(DomesticAddress1, DomesticAddress2, Package1SignatureRequired);

            // Assert that we have a non-signature response
            Assert.NotNull(nonSignatureResponse);
            Assert.IsNotEmpty(nonSignatureResponse.Rates);
            Assert.IsEmpty(nonSignatureResponse.Errors);
            Assert.True(nonSignatureResponse.Rates.First().TotalCharges > 0);

            // Assert that we have a signature response
            Assert.NotNull(signatureResponse);
            Assert.IsNotEmpty(signatureResponse.Rates);
            Assert.IsEmpty(signatureResponse.Errors);
            Assert.True(signatureResponse.Rates.First().TotalCharges > 0);

            // Now compare prices
            foreach (var signatureRate in signatureResponse.Rates)
            {
                var nonSignatureRate = nonSignatureResponse.Rates.FirstOrDefault(x => x.Name == signatureRate.Name);

                if (nonSignatureRate != null)
                {
                    var signatureTotalCharges = signatureRate.TotalCharges;
                    var nonSignatureTotalCharges = nonSignatureRate.TotalCharges;
                    Assert.AreNotEqual(signatureTotalCharges, nonSignatureTotalCharges);
                }
            }
        }

        [Test]
        public async Task USPS_Domestic_Saturday_Delivery()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSProvider(_uspsUserId));

            var today = DateTime.Now;
            var nextFriday = today.AddDays(12 - (int)today.DayOfWeek);

            var origin = new Address("", "", "06405", "US");
            var destination = new Address("", "", "20852", "US");

            var response = await rateManager.GetRatesAsync(origin, destination, Package1, new ShipmentOptions()
            {
                ShippingDate = nextFriday,
                SaturdayDelivery = true
            });

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.IsNotEmpty(response.Rates);
            Assert.True(response.Rates.Any(r => r.Options.SaturdayDelivery));
            Assert.IsEmpty(response.Errors);

            foreach (var rate in response.Rates)
            {
                Assert.NotNull(rate);
                Assert.True(rate.TotalCharges > 0);

                Debug.WriteLine(rate.Name + ": " + rate.TotalCharges);
            }
        }
    }
}
