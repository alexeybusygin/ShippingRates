using NUnit.Framework;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using ShippingRates.ShippingProviders;

namespace ShippingRates.Tests.ShippingProviders
{
    public abstract class DHLRatesTestsBase
    {
        protected readonly RateManager _rateManager;
        protected readonly DHLProvider _provider;

        protected DHLRatesTestsBase()
        {
            _rateManager = GetRateManager(GetConfiguration());
        }

        protected DHLProviderConfiguration GetConfiguration()
        {
            var config = GetApplicationConfiguration();

            return new DHLProviderConfiguration(config.DHLSiteId, config.DHLPassword, false);
        }

        protected TestsConfiguration GetApplicationConfiguration() =>
            ConfigHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);

        protected RateManager GetRateManager(DHLProviderConfiguration configuration)
        {
            var provider = new DHLProvider(configuration);

            var rateManager = new RateManager();
            rateManager.AddProvider(provider);

            return rateManager;
        }
    }

    [TestFixture]
    public class DHLRates : DHLRatesTestsBase
    {
        [Test]
        public void DHLReturnsRates()
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");      // Test for a decimal format

            var from = new Address("", "", "75003", "FR");
            var to = new Address("", "", "53711", "US");
            var package = new Package(7, 7, 7, 6, 0);

            var r = _rateManager.GetRates(from, to, package);
            var rates = r.Rates.ToList();

            Assert.NotNull(r);
            Assert.True(rates.Any());

            foreach (var rate in rates)
            {
                Assert.True(rate.TotalCharges > 0);
            }
        }

        [Test]
        public void DHLIncludeAndExcludeServices()
        {
            var from = new Address("", "", "75003", "FR");
            var to = new Address("", "", "75011", "FR");
            var package = new Package(7, 7, 7, 6, 0);

            var configuration1 = GetConfiguration().IncludeServices(new char[] { 'I' });

            var r1 = GetRateManager(configuration1).GetRates(from, to, package);
            var rates1 = r1.Rates.ToList();

            Assert.True((rates1?.Count() ?? 0) == 1);
            Assert.True(rates1.Any(r => r.Name.Contains("DOMESTIC 9:00")));

            var configuration2 = GetConfiguration().ExcludeServices(new char[] { 'C' });

            var r2 = GetRateManager(configuration2).GetRates(from, to, package);
            var rates2 = r2.Rates.ToList();

            Assert.True(rates2?.Any() ?? false);
            Assert.False(rates2.Any(r => r.Name.Contains("MEDICAL")));
        }

        [Test]
        public void DHLIncludePaymentAccountNumber()
        {
            var from = new Address("", "", "75003", "FR");
            var to = new Address("", "", "75011", "FR");
            var package = new Package(7, 7, 7, 6, 0);

            var r1 = _rateManager.GetRates(from, to, package);
            var rates1 = r1.Rates.ToList();

            var configuration2 = GetConfiguration();
            configuration2.PaymentAccountNumber = GetApplicationConfiguration().DHLAccountNumber;

            var r2 = GetRateManager(configuration2).GetRates(from, to, package);
            var rates2 = r2.Rates.ToList();

            Assert.True(rates1?.Any() ?? false);
            Assert.True(rates2?.Any() ?? false);
            Assert.True(
                rates1.Count() != rates2.Count() ||
                rates1.Sum(r => r.TotalCharges) != rates2.Sum(r => r.TotalCharges));
        }

        [Test]
        public void DHLReturnsErrors()
        {
            var from = new Address("", "", "21401", "US");
            var to = new Address("", "", "30404", "US");
            var package = new Package(7, 7, 7, 6, 1);

            var r = _rateManager.GetRates(from, to, package);
            var rates = r.Rates.ToList();

            Assert.NotNull(r);
            Assert.False(rates.Any());
            Assert.True(r.Errors.Any());

            var error = r.Errors.FirstOrDefault(r => r.Number == "420505");
            Assert.NotNull(error);
            Assert.NotNull(error.Description);
            Assert.AreEqual(error.Description.Substring(0, 35), "The destination location is invalid");
        }

        /*
        [Test]
        public async Task DHLSaturdayDelivery()
        {
            var from = new Address("", "", "75003", "FR");  // Paris to Lyon
            var to = new Address("", "", "69009", "FR");
            //var package = new Package(7, 7, 7, 6, 1);
            var package = new DocumentsPackage(5, 1);

            var today = DateTime.Now;
            var nextFriday = today.AddDays(11 - (int)today.DayOfWeek);

            var r = await _rateManager.GetRatesAsync(from, to, package, new ShipmentOptions()
            {
                ShippingDate = nextFriday,
                SaturdayDelivery = true
            });
            var rates = r.Rates.ToList();

            Assert.NotNull(r);
            Assert.True(rates.Any());
            Assert.True(rates.Any(r => r.Options.SaturdayDelivery));
        }
        */

        [Test]
        public async Task DHLCurrency()
        {
            var from = new Address("Amsterdam", "", "1043 AG", "NL");
            var to = new Address("London", "", "SW1A 2AA", "GB");
            var package = new Package(1, 1, 1, 5, 1);

            var r = await _rateManager.GetRatesAsync(from, to, package);
            var dhlRates = r.Rates.ToList();

            Assert.NotNull(r);
            Assert.True(dhlRates.Any());
            Assert.False(dhlRates.Any(r => r.CurrencyCode != "EUR"));
        }

        [Test]
        public void CanGetDHLServiceCodes()
        {
            var serviceCodes = DHLProvider.AvailableServices;

            Assert.NotNull(serviceCodes);
            Assert.IsNotEmpty(serviceCodes);
        }
    }
}
