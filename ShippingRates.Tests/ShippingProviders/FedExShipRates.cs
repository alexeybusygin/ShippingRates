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

            Assert.Multiple(() =>
            {
                Assert.That(r, Is.Not.Null);
                Assert.That(fedExRates, Is.Not.Empty);
            });

            foreach (var rate in fedExRates)
            {
                Assert.That(rate.TotalCharges, Is.GreaterThan(0));
            }
        }

        [Test]
        public void FedExReturnsErrors()
        {
            var from = new Address("Annapolis", "MD", "21401", "US");
            var to = new Address("", "", "30404", "US");
            var package = new Package(7, 7, 7, 6, 1);

            var rates = _rateManager.GetRates(from, to, package);

            Assert.Multiple(() =>
            {
                Assert.That(rates, Is.Not.Null);
                Assert.That(rates.Rates, Is.Empty);
            });
            Assert.That(rates.Errors, Has.Count.EqualTo(1));

            var error = rates.Errors.FirstOrDefault();
            Assert.That(error, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(error.Number, Is.EqualTo("521"));
                Assert.That(error.Description, Is.Not.Null);
            });
            Assert.That(error.Description[..42], Is.EqualTo("Destination postal code missing or invalid"));
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

            Assert.Multiple(() =>
            {
                Assert.That(rates.Rates, Is.Not.Empty);
                Assert.That(rates.Rates.All(r => r.Name.Contains("Freight")), Is.True);
            });
        }

        private static void AssertRatesAreNotEqual(Shipment r1, Shipment r2, string methodCode = null)
        {
            Assert.Multiple(() =>
            {
                Assert.That(r1?.Rates, Is.Not.Null);
                Assert.That(r2?.Rates, Is.Not.Null);
            });
            Assert.Multiple(() =>
            {
                Assert.That(r1.Rates, Is.Not.Empty);
                Assert.That(r2.Rates, Is.Not.Empty);
            });

            var commonCode = methodCode ?? r1.Rates
                .Select(r => r.ProviderCode)
                .Where(c => r2.Rates.Select(r => r.ProviderCode).Contains(c))
                .FirstOrDefault();
            Assert.That(commonCode, Is.Not.Null);

            var rate1 = r1.Rates.FirstOrDefault(r => r.ProviderCode == commonCode);
            var rate2 = r2.Rates.FirstOrDefault(r => r.ProviderCode == commonCode);
            Assert.Multiple(() =>
            {
                Assert.That(rate1, Is.Not.Null);
                Assert.That(rate2, Is.Not.Null);
            });
            Assert.That(rate2.TotalCharges, Is.Not.EqualTo(rate1.TotalCharges));
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

            Assert.That(r, Is.Not.Null);
            Assert.That(r.Rates, Is.Not.Empty);
            Assert.That(r.Rates.Any(r => r.Options.SaturdayDelivery), Is.True);
        }

        [Test]
        public async Task FedExCurrency()
        {
            var from = new Address("", "", "220-8515", "JP");
            var to = new Address("", "", "058357", "SG");
            var package = new Package(1, 1, 1, 5, 1);

            var r = await _rateManager.GetRatesAsync(from, to, package);

            Assert.That(r, Is.Not.Null);
            Assert.That(r.Rates, Is.Not.Empty);
            Assert.That(r.Rates.Any(r => r.TotalCharges > 1000), Is.False);
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

            Assert.That(r, Is.Not.Null);
            Assert.That(r.Rates, Is.Not.Empty);
            Assert.That(r.Rates.Any(r => r.CurrencyCode != "USD"), Is.False);

            var rEuro = await _rateManager.GetRatesAsync(from, to, package, new ShipmentOptions()
            {
                PreferredCurrencyCode = "EUR"
            });
            var fedExEuroRates = rEuro.Rates.ToList();

            Assert.That(rEuro, Is.Not.Null);
            Assert.That(rEuro.Rates, Is.Not.Empty);
            Assert.That(rEuro.Rates.Any(r => r.CurrencyCode != "EUR"), Is.False);
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

            Assert.That(serviceCodes, Is.Not.Null);
            Assert.That(serviceCodes, Is.Not.Empty);
        }
    }
}
