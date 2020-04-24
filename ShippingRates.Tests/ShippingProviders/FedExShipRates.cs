using NUnit.Framework;
using System;
using System.Linq;

using ShippingRates.ShippingProviders;

namespace ShippingRates.Tests.ShippingProviders
{
    public abstract class FedExShipRatesTestsBase
    {
        protected readonly RateManager _rateManager;
        protected readonly FedExProvider _provider;

        protected FedExShipRatesTestsBase()
        {
            var config = ConfigHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);

            var fedexKey = config.FedExKey;
            var fedexPassword = config.FedExPassword;
            var fedexAccountNumber = config.FedExAccountNumber;
            var fedexMeterNumber = config.FedExMeterNumber;
            var fedexUseProduction = config.FedExUseProduction;

            _provider = new FedExProvider(fedexKey, fedexPassword, fedexAccountNumber, fedexMeterNumber, fedexUseProduction);

            _rateManager = new RateManager();
            _rateManager.AddProvider(_provider);
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
