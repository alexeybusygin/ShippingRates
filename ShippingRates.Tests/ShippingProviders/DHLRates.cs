using NUnit.Framework;
using ShippingRates.ShippingProviders;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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

        protected static DHLProviderConfiguration GetConfiguration()
        {
            var config = GetApplicationConfiguration();

            return new DHLProviderConfiguration(config.DHLSiteId, config.DHLPassword, false);
        }

        protected static TestsConfiguration GetApplicationConfiguration() =>
            ConfigHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);

        protected static RateManager GetRateManager(DHLProviderConfiguration configuration)
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
            Assert.That(r, Is.Not.Null);
            Assert.That(r.Rates, Is.Not.Empty);

            foreach (var rate in r.Rates)
            {
                Assert.That(rate.TotalCharges, Is.GreaterThan(0));
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
            Assert.That(r1, Is.Not.Null);
            Assert.That(r1.Rates, Is.Not.Empty);
            Assert.That(r1.Rates, Has.Some.Matches<Rate>(r => r.Name.Contains("DOMESTIC 9:00")));

            var configuration2 = GetConfiguration().ExcludeServices(new char[] { 'C' });

            var r2 = GetRateManager(configuration2).GetRates(from, to, package);
            Assert.That(r2, Is.Not.Null);
            Assert.That(r2.Rates, Is.Not.Empty);
            Assert.That(r2.Rates, Has.None.Matches<Rate>(r => r.Name.Contains("MEDICAL")));
        }

        [Test]
        public void DHLIncludePaymentAccountNumber()
        {
            var from = new Address("", "", "75003", "FR");
            var to = new Address("", "", "75011", "FR");
            var package = new Package(7, 7, 7, 6, 0);

            var r1 = _rateManager.GetRates(from, to, package);

            var configuration2 = GetConfiguration();
            configuration2.PaymentAccountNumber = GetApplicationConfiguration().DHLAccountNumber;

            var r2 = GetRateManager(configuration2).GetRates(from, to, package);

            Assert.Multiple(() =>
            {
                Assert.That(r1.Rates, Is.Not.Empty);
                Assert.That(r2.Rates, Is.Not.Empty);
            });

            Assert.That(
                r1.Rates.Count != r2.Rates.Count ||
                r1.Rates.Sum(r => r.TotalCharges) != r2.Rates.Sum(r => r.TotalCharges),
                Is.True);
        }

        [Test]
        public void DHLReturnsErrors()
        {
            var from = new Address("", "", "21401", "US");
            var to = new Address("", "", "30404", "US");
            var package = new Package(7, 7, 7, 6, 1);

            var r = _rateManager.GetRates(from, to, package);
            Assert.That(r, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(r.Errors, Is.Not.Empty);
                Assert.That(r.Rates, Is.Empty);
            });

            var error = r.Errors.FirstOrDefault(r => r.Number == "420505");
            Assert.That(error, Is.Not.Null);
            Assert.That(error.Description, Is.Not.Null);
            Assert.That(error.Description[..35], Is.EqualTo("The destination location is invalid"));
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
            Assert.That(r, Is.Not.Null);

            var dhlRates = r.Rates.ToList();
            Assert.That(dhlRates, Is.Not.Empty);
            Assert.That(dhlRates, Has.All.Matches<Rate>(r => r.CurrencyCode == "EUR"));
        }

        [Test]
        public void CanGetDHLServiceCodes()
        {
            var serviceCodes = DHLProvider.AvailableServices;

            Assert.That(serviceCodes, Is.Not.Null);
            Assert.That(serviceCodes, Is.Not.Empty);
        }
    }
}
