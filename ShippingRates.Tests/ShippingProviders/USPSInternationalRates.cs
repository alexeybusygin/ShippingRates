using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using ShippingRates.ShippingProviders;

namespace ShippingRates.Tests.ShippingProviders
{
    [TestFixture]
    public class USPSInternationalRates
    {
        private const decimal FirstClassLetterMaxWeight = 0.21875m; // 3.5 ounces

        private readonly Address _domesticAddress1;
        private readonly Address _domesticAddress2;
        private readonly Address _internationalAddress1;
        private readonly Address _internationalAddress2;
        private readonly DocumentsPackage _firstClassLetterWithNoValue;
        private readonly DocumentsPackage _firstClassLetterWithValue;
        private readonly Package _package1;
        private readonly Package _package2;
        private readonly string _uspsUserId;

        public USPSInternationalRates()
        {
            _domesticAddress1 = new Address("278 Buckley Jones Road", "", "", "Cleveland", "MS", "38732", "US");
            _domesticAddress2 = new Address("One Microsoft Way", "", "", "Redmond", "WA", "98052", "US");
            _internationalAddress1 = new Address("Jubail", "Jubail", "31951", "SA"); //has limited intl services available
            _internationalAddress2 = new Address("80-100 Victoria St", "", "", "London", "", "SW1E 5JL", "GB");

            _firstClassLetterWithNoValue = new DocumentsPackage(FirstClassLetterMaxWeight, 0);
            _firstClassLetterWithValue = new DocumentsPackage(FirstClassLetterMaxWeight, 1);
            _package1 = new Package(14, 14, 14, 15, 0);
            _package2 = new Package(6, 6, 6, 5, 100);

            _uspsUserId = ConfigHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory)
                .USPSUserId;
        }

        [Test]
        public void USPS_Intl_Returns_Multiple_Rates_When_Using_Multiple_Packages_For_All_Services_And_Multiple_Packages()
        {
            var packages = new List<Package>();
            packages.Add(_package1);
            packages.Add(_package2);

            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSInternationalProvider(_uspsUserId));

            var response = rateManager.GetRates(_domesticAddress1, _internationalAddress2, packages);

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
        public void USPS_Intl_Returns_Multiple_Rates_When_Using_Valid_Addresses_For_All_Services()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSInternationalProvider(_uspsUserId));

            var response = rateManager.GetRates(_domesticAddress1, _internationalAddress2, _package1);

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
        public void USPS_Intl_Returns_No_Rates_When_Using_Invalid_Addresses_For_All_Services()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSInternationalProvider(_uspsUserId));

            var response = rateManager.GetRates(_domesticAddress1, _domesticAddress2, _package1);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.IsEmpty(response.Rates);
        }

        [Test]
        public void USPS_Intl_Returns_No_Rates_When_Using_Invalid_Addresses_For_Single_Service()
        {
            //can't rate intl with a domestic address

            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSInternationalProvider(_uspsUserId, "Priority Mail International"));

            var response = rateManager.GetRates(_domesticAddress1, _domesticAddress2, _package1);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.IsEmpty(response.Rates);
        }

        [Test]
        public void USPS_Intl_Returns_Single_Rate_When_Using_Valid_Addresses_For_Single_Service()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSInternationalProvider(_uspsUserId, "Priority Mail International"));

            var response = rateManager.GetRates(_domesticAddress1, _internationalAddress2, _package1);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.IsNotEmpty(response.Rates);
            Assert.IsEmpty(response.Errors);
            Assert.AreEqual(response.Rates.Count, 1);
            Assert.True(response.Rates.First().TotalCharges > 0);

            Debug.WriteLine(response.Rates.First().Name + ": " + response.Rates.First().TotalCharges);
        }

        [Test]
        public void USPS_Intl_Returns_First_Class_Mail_Rates_For_FirstClassLetterWithNoValue()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSInternationalProvider(_uspsUserId));

            var response = rateManager.GetRates(_domesticAddress1, _internationalAddress2, _firstClassLetterWithNoValue);
            Assert.True(response.Rates.Any(IsFirstClassMailRate));
        }

        [Test]
        public void USPS_Intl_Returns_No_First_Class_Mail_Rates_For_FirstClassLetterWithValue()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSInternationalProvider(_uspsUserId));

            var response = rateManager.GetRates(_domesticAddress1, _internationalAddress2, _firstClassLetterWithValue);
            Assert.False(response.Rates.Any(IsFirstClassMailRate));
        }

        [Test]
        public void USPS_Intl_Returns_No_First_Class_Mail_Rates_For_Package()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSInternationalProvider(_uspsUserId));

            var package = new Package(7m, 5m, 0.1m, FirstClassLetterMaxWeight, 0);

            var response = rateManager.GetRates(_domesticAddress1, _internationalAddress2, package);
            Assert.False(response.Rates.Any(IsFirstClassMailRate));
        }

        [Test]
        public void CanGetUspsInternationalServiceCodes()
        {
            var provider = new USPSInternationalProvider(_uspsUserId);
            var serviceCodes = provider.GetServiceCodes();

            Assert.NotNull(serviceCodes);
            Assert.IsNotEmpty(serviceCodes);
        }

        [Test]
        public async Task USPSCurrency()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSInternationalProvider(_uspsUserId));

            var response = await rateManager.GetRatesAsync(_domesticAddress1, _internationalAddress2, _package1);
            var rates = response.Rates;

            Assert.NotNull(response);
            Assert.True(rates.Any());
            Assert.False(rates.Any(r => r.CurrencyCode != "USD"));
        }

        private bool IsFirstClassMailRate(Rate rate)
        {
            return rate.ProviderCode is "First-Class Mail International Letter" or
                "First-Class Mail International Large Envelope";
        }
    }
}
