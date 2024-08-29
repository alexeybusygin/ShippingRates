using NUnit.Framework;

using ShippingRates.ShippingProviders;

namespace ShippingRates.Tests.ShippingProviders
{
    public abstract class FedExSmartPostShipRatesTestsBase
    {
        protected readonly RateManager _rateManager;
        protected readonly FedExSmartPostProvider _provider;

        protected FedExSmartPostShipRatesTestsBase()
        {
            var config = ConfigHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);

            _provider = new FedExSmartPostProvider(new FedExProviderConfiguration()
            {
                Key = config.FedExKey,
                Password = config.FedExPassword,
                AccountNumber = config.FedExAccountNumber,
                MeterNumber = config.FedExMeterNumber,
                HubId = config.FedExHubId,
                UseProduction = config.FedExUseProduction
            });

            _rateManager = new RateManager();
            _rateManager.AddProvider(_provider);
        }
    }

    [TestFixture]
    public class FedExSmartPostShipRates : FedExSmartPostShipRatesTestsBase
    {
        /*
        [Test]
        public void FedExSmartPostReturnsRates()
        {
            var from = new Address("Annapolis", "MD", "21401", "US");
            var to = new Address("Fitchburg", "WI", "53711", "US");
            var package = new Package(7, 7, 7, 6, 0);

            var r = _rateManager.GetRates(from, to, package);
            var fedExRates = r.Rates.ToList();

            Assert.NotNull(r);
            Assert.True(fedExRates.Any());

            foreach (var rate in fedExRates)
            {
                Assert.True(rate.TotalCharges > 0);
                Assert.AreEqual(rate.ProviderCode, "SMART_POST");
            }
        }
        */

        [Test]
        public void CanGetFedExServiceCodes()
        {
            var serviceCodes = _provider.GetServiceCodes();

            Assert.That(serviceCodes, Is.Not.Null);
            Assert.That(serviceCodes, Is.Not.Empty);
        }
    }
}
