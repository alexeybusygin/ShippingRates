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
            var fedExRates = r.Rates.ToList();

            Assert.NotNull(r);
            Assert.True(fedExRates.Any());

            foreach (var rate in fedExRates)
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
            var fedExRates = r.Rates.ToList();

            Assert.NotNull(r);
            Assert.False(fedExRates.Any());
            Assert.AreEqual(r.Errors.Count(), 3);

            var error = r.Errors.FirstOrDefault(r => r.Number == "420505");
            Assert.NotNull(error);
            Assert.NotNull(error.Description);
            Assert.AreEqual(error.Description.Substring(0, 35), "The destination location is invalid");
        }

        [Test]
        public async Task DHLSaturdayDelivery()
        {
            var from = new Address("", "", "75003", "FR");  // Paris to Lyon
            var to = new Address("", "", "69009", "FR");
            var package = new Package(7, 7, 7, 6, 0);

            var today = DateTime.Now;
            var nextFriday = today.AddDays(12 - (int)today.DayOfWeek);

            var r = await _rateManager.GetRatesAsync(from, to, package, new ShipmentOptions()
            {
                ShippingDate = nextFriday,
                SaturdayDelivery = true
            });
            var fedExRates = r.Rates.ToList();

            Assert.NotNull(r);
            Assert.True(fedExRates.Any());
            Assert.True(fedExRates.Any(r => r.Options.SaturdayDelivery));
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
