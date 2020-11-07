using NUnit.Framework;
using System;
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
            var config = ConfigHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);

            _provider = new DHLProvider(config.DHLSiteId, config.DHLPassword, false);

            _rateManager = new RateManager();
            _rateManager.AddProvider(_provider);
        }
    }

    [TestFixture]
    public class DHLRates : DHLRatesTestsBase
    {
        [Test]
        public void DHLReturnsRates()
        {
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
        public void DHLReturnsErrors()
        {
            var from = new Address("", "", "21401", "US");
            var to = new Address("", "", "30404", "US");
            var package = new Package(7, 7, 7, 6, 1);

            var r = _rateManager.GetRates(from, to, package);
            var rates = r.Rates.ToList();

            Assert.NotNull(r);
            Assert.False(rates.Any());
            Assert.AreEqual(r.Errors.Count(), 3);

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
