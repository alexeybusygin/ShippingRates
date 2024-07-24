using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

using ShippingRates.ShippingProviders;

namespace ShippingRates.Tests.ShippingProviders
{
    public abstract class FedExShipRatesTestsBase
    {
        protected readonly RateManager _rateManager;
        protected readonly RateManager _rateManagerNegotiated;
        protected readonly FedExProvider _provider;
        protected readonly FedExProvider _providerNegotiated;

        protected FedExShipRatesTestsBase()
        {
            var config = ConfigHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);

            _provider = new FedExProvider(new FedExProviderConfiguration()
            {
                Key = config.FedExKey,
                Password = config.FedExPassword,
                AccountNumber = config.FedExAccountNumber,
                MeterNumber = config.FedExMeterNumber,
                UseProduction = config.FedExUseProduction
            });

            _rateManager = new RateManager();
            _rateManager.AddProvider(_provider);

            _providerNegotiated = new FedExProvider(new FedExProviderConfiguration()
            {
                Key = config.FedExKey,
                Password = config.FedExPassword,
                AccountNumber = config.FedExAccountNumber,
                MeterNumber = config.FedExMeterNumber,
                UseProduction = config.FedExUseProduction,
                UseNegotiatedRates = true
            });

            _rateManagerNegotiated = new RateManager();
            _rateManagerNegotiated.AddProvider(_providerNegotiated);
        }
    }

    [TestFixture]
    public class FedExShipRates : FedExShipRatesTestsBase
    {
        [Test]
        public void FedExReturnsRates()
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
            }
        }

        [Test]
        public void FedExReturnsErrors()
        {
            var from = new Address("Annapolis", "MD", "21401", "US");
            var to = new Address("", "", "30404", "US");
            var package = new Package(7, 7, 7, 6, 1);

            var r = _rateManager.GetRates(from, to, package);
            var fedExRates = r.Rates.ToList();

            Assert.NotNull(r);
            Assert.False(fedExRates.Any());
            Assert.AreEqual(r.Errors.Count(), 1);

            var error = r.Errors.FirstOrDefault();
            Assert.NotNull(error);
            Assert.AreEqual(error.Number, "521");
            Assert.NotNull(error.Description);
            Assert.AreEqual(error.Description.Substring(0, 42), "Destination postal code missing or invalid");
        }


        [Test]
        public void FedExNegotiatedRates()
        {
            var from = new Address("Annapolis", "MD", "21401", "US");
            var to = new Address("Fitchburg", "WI", "53711", "US");
            var package = new Package(7, 7, 7, 6, 1);

            var r = _rateManager.GetRates(from, to, package);
            var rN = _rateManagerNegotiated.GetRates(from, to, package);

            AssertRatesAreNotEqual(r, rN, "FEDEX_GROUND");
        }

        [Test]
        public void FedExOneRate()
        {
            var from = new Address("Annapolis", "MD", "21401", "US");
            var to = new Address("Fitchburg", "WI", "53711", "US");
            var package = new Package(7, 7, 7, 6, 1);


            var rates = _rateManager.GetRates(from, to, package);
            var oneRates = _rateManagerNegotiated.GetRates(from, to, package, new ShipmentOptions()
            {
                FedExOneRate = true
            });

            AssertRatesAreNotEqual(rates, oneRates);
        }

        [Test]
        public void FedExOneRatePackage()
        {
            var from = new Address("Annapolis", "MD", "21401", "US");
            var to = new Address("Fitchburg", "WI", "53711", "US");
            var package = new Package(7, 7, 7, 6, 1);

            var rates = _rateManagerNegotiated.GetRates(from, to, package, new ShipmentOptions()
            {
                FedExOneRate = true,
                //if not set, will default to FEDEX_MEDIUM_BOX
            });
            var oneRates = _rateManagerNegotiated.GetRates(from, to, package, new ShipmentOptions()
            {
                FedExOneRate = true,
                FedExOneRatePackageOverride ="FEDEX_ENVELOPE" //one of the cheapest options
            });

            AssertRatesAreNotEqual(rates, oneRates);
        }

        [Test]
        public void FedExFreight()
        {
            var from = new Address("Annapolis", "MD", "21401", "US");
            var to = new Address("Fitchburg", "WI", "53711", "US");
            var package = new Package(48, 48, 48, 120, 100);

            var rates = _rateManager.GetRates(from, to, package);

            Assert.True(rates.Rates.Any());
            Assert.True(!rates.Rates.Any(r => !r.Name.Contains("Freight")));
        }

        private void AssertRatesAreNotEqual(Shipment r1, Shipment r2, string methodCode = null)
        {
            Assert.NotNull(r1?.Rates);
            Assert.NotNull(r2?.Rates);
            Assert.True(r1.Rates.Any());
            Assert.True(r2.Rates.Any());

            var commonCode = methodCode ?? r1.Rates
                .Select(r => r.ProviderCode)
                .Where(c => r2.Rates.Select(r => r.ProviderCode).Contains(c))
                .FirstOrDefault();
            Assert.NotNull(commonCode);

            var rate1 = r1.Rates.FirstOrDefault(r => r.ProviderCode == commonCode);
            var rate2 = r2.Rates.FirstOrDefault(r => r.ProviderCode == commonCode);
            Assert.NotNull(rate1);
            Assert.NotNull(rate2);
            Assert.AreNotEqual(rate1.TotalCharges, rate2.TotalCharges);
        }

        [Test]
        public async Task FedExSaturdayDelivery()
        {
            var from = new Address("Annapolis", "MD", "21401", "US");
            var to = new Address("Fitchburg", "WI", "53711", "US");
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
        public async Task FedExCurrency()
        {
            var from = new Address("", "", "220-8515", "JP");
            var to = new Address("", "", "058357", "SG");
            var package = new Package(1, 1, 1, 5, 1);

            var r = await _rateManager.GetRatesAsync(from, to, package);
            var fedExRates = r.Rates.ToList();

            Assert.NotNull(r);
            Assert.True(fedExRates.Any());
            Assert.False(fedExRates.Any(r => r.TotalCharges > 1000));
        }

        [Test]
        public async Task FedExPreferredCurrency()
        {
            var from = new Address("Amsterdam", "", "1043 AG", "NL");
            var to = new Address("London", "", "SW1A 2AA", "GB");
            var package = new Package(1, 1, 1, 5, 1);

            var r = await _rateManager.GetRatesAsync(from, to, package, new ShipmentOptions()
            {
                PreferredCurrencyCode = "USD"
            });
            var fedExRates = r.Rates.ToList();

            Assert.NotNull(r);
            Assert.True(fedExRates.Any());
            Assert.False(fedExRates.Any(r => r.CurrencyCode != "USD"));

            var rEuro = await _rateManager.GetRatesAsync(from, to, package, new ShipmentOptions()
            {
                PreferredCurrencyCode = "EUR"
            });
            var fedExEuroRates = rEuro.Rates.ToList();

            Assert.NotNull(rEuro);
            Assert.True(fedExEuroRates.Any());
            Assert.False(fedExEuroRates.Any(r => r.CurrencyCode != "EUR"));
        }

        /*
         * According to docs (24.2.1): "Direct Signature Required is the default service and is
         * provided at no additional cost."
         * 
        [Test]
        public void FedExReturnsDifferentRatesForSignatureOnDelivery()
        {
            var from = new Address("Annapolis", "MD", "21401", "US");
            var to = new Address("Fitchburg", "WI", "53711", "US");

            var nonSignaturePackage = new Package(7, 7, 7, 6, 0, null, false);
            var signaturePackage = new Package(7, 7, 7, 6, 0, null, true);

            // Non signature rates first
            var nonSignatureRates = _rateManager.GetRates(from, to, nonSignaturePackage);
            var fedExNonSignatureRates = nonSignatureRates.Rates.ToList();

            Assert.NotNull(nonSignatureRates);
            Assert.True(fedExNonSignatureRates.Any());

            foreach (var rate in fedExNonSignatureRates)
            {
                Assert.True(rate.TotalCharges > 0);
            }
            
            var signatureRates = _rateManager.GetRates(from, to, signaturePackage);
            var fedExSignatureRates = signatureRates.Rates.ToList();

            Assert.NotNull(signatureRates);
            Assert.True(fedExSignatureRates.Any());

            foreach (var rate in fedExSignatureRates)
            {
                Assert.True(rate.TotalCharges > 0);
            }

            // Now compare prices
            foreach (var signatureRate in fedExSignatureRates)
            {
                var nonSignatureRate = fedExNonSignatureRates.FirstOrDefault(x => x.Name == signatureRate.Name);

                if (nonSignatureRate != null)
                {
                    var signatureTotalCharges = signatureRate.TotalCharges;
                    var nonSignatureTotalCharges = nonSignatureRate.TotalCharges;
                    Assert.AreNotEqual(signatureTotalCharges, nonSignatureTotalCharges);
                }
            }
        }*/

        [Test]
        public void CanGetFedExServiceCodes()
        {
            var serviceCodes = _provider.GetServiceCodes();

            Assert.NotNull(serviceCodes);
            Assert.IsNotEmpty(serviceCodes);
        }
    }
}
