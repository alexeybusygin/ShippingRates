using NUnit.Framework;
using ShippingRates.Models.UPS;
using ShippingRates.ShippingProviders;
using System.Collections.Generic;

namespace ShippingRates.Tests.Models.UPS
{
    [TestFixture()]
    internal class UpsRatingRequestBuilderTests
    {
        [Test()]
        public void UpsRatingRequestBuilder_CustomerClassification()
        {
            // US shipment
            var from = new Address("Annapolis", "MD", "21401", "US");
            var to = new Address("", "", "30404", "US");

            var package1 = new Package(1, 1, 1, 3, 0);
            var package2 = new Package(1, 2, 3, 4, 5);

            var shipment = new Shipment(from, to, new List<Package>() { package1, package2 });

            // International shipment
            var fromNL = new Address("Amsterdam", "", "1043 AG", "NL");
            var shipmentNL = new Shipment(fromNL, to, new List<Package>() { package1, package2 });

            // Daily rates
            var dailyRatesConfig = new UPSProviderConfiguration()
            {
                UseDailyRates = true
            };
            var dailyRatesBuilder = new UpsRatingRequestBuilder(dailyRatesConfig);
            var dailyRatesRequest = dailyRatesBuilder.Build(shipment);
            Assert.That(dailyRatesRequest?.RateRequest?.CustomerClassification, Is.Not.Null);
            Assert.That(dailyRatesRequest?.RateRequest?.CustomerClassification.Code, Is.EqualTo("01"));

            // Retail rates
            var retailRatesConfig = new UPSProviderConfiguration()
            {
                UseRetailRates = true
            };
            var retailRatesBuilder = new UpsRatingRequestBuilder(retailRatesConfig);
            var retailRatesRequest = retailRatesBuilder.Build(shipment);
            Assert.That(retailRatesRequest?.RateRequest?.CustomerClassification, Is.Not.Null);
            Assert.That(retailRatesRequest?.RateRequest?.CustomerClassification.Code, Is.EqualTo("04"));

            // Regular rates
            var regularRatesConfig = new UPSProviderConfiguration();
            var regularRatesBuilder = new UpsRatingRequestBuilder(regularRatesConfig);
            var regularRatesRequest = regularRatesBuilder.Build(shipment);
            Assert.That(regularRatesRequest?.RateRequest?.CustomerClassification, Is.Not.Null);
            Assert.That(regularRatesRequest?.RateRequest?.CustomerClassification.Code, Is.EqualTo("00"));

            // International shipment
            var internationalRequest = dailyRatesBuilder.Build(shipmentNL);
            Assert.That(internationalRequest?.RateRequest?.CustomerClassification, Is.Null);
        }
    }
}
