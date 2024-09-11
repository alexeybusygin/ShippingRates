using NUnit.Framework;
using ShippingRates.Models.UPS;
using ShippingRates.ShippingProviders;
using System.Collections.Generic;

namespace ShippingRates.Tests.Models.UPS
{
    [TestFixture()]
    internal class UpsRatingRequestBuilderTests
    {
        readonly Address AddressFrom = new("Annapolis", "MD", "21401", "US");
        readonly Address AddressTo = new("", "", "30404", "US");
        readonly List<Package> Packages = new() { new(1, 1, 1, 3, 0), new(1, 2, 3, 4, 5) };

        [Test()]
        public void UpsRatingRequestBuilder_CustomerClassification()
        {
            // US shipment
            var shipment = new Shipment(AddressFrom, AddressTo, Packages);

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
        }


        [Test()]
        public void UpsRatingRequestBuilder_CustomerClassificationDefaults()
        {
            // US shipment
            var shipment = new Shipment(AddressFrom, AddressTo, Packages);

            var regularRatesConfig = new UPSProviderConfiguration();
            var regularRatesBuilder = new UpsRatingRequestBuilder(regularRatesConfig);
            var regularRatesRequest = regularRatesBuilder.Build(shipment);
            Assert.That(regularRatesRequest?.RateRequest?.CustomerClassification, Is.Not.Null);
            Assert.That(regularRatesRequest?.RateRequest?.CustomerClassification.Code, Is.EqualTo("00"));

            // International shipment
            var fromNL = new Address("Amsterdam", "", "1043 AG", "NL");
            var shipmentNL = new Shipment(fromNL, AddressTo, Packages);

            var internationalRequest = regularRatesBuilder.Build(shipmentNL);
            Assert.That(internationalRequest?.RateRequest?.CustomerClassification, Is.Null);
        }


        [Test()]
        public void UpsRatingRequestBuilder_CustomerClassificationFromCode()
        {
            // US shipment
            var shipment = new Shipment(AddressFrom, AddressTo, Packages);

            // Daily rates
            var dailyRatesConfig = new UPSProviderConfiguration()
            {
                CustomerClassification = UPSCustomerClassification.DailyRates
            };
            var dailyRatesBuilder = new UpsRatingRequestBuilder(dailyRatesConfig);
            var dailyRatesRequest = dailyRatesBuilder.Build(shipment);
            Assert.That(dailyRatesRequest?.RateRequest?.CustomerClassification, Is.Not.Null);
            Assert.That(dailyRatesRequest?.RateRequest?.CustomerClassification.Code, Is.EqualTo("01"));

            // Standard rates
            var retailRatesConfig = new UPSProviderConfiguration()
            {
                CustomerClassification = UPSCustomerClassification.StandardListRates
            };
            var retailRatesBuilder = new UpsRatingRequestBuilder(retailRatesConfig);
            var retailRatesRequest = retailRatesBuilder.Build(shipment);
            Assert.That(retailRatesRequest?.RateRequest?.CustomerClassification, Is.Not.Null);
            Assert.That(retailRatesRequest?.RateRequest?.CustomerClassification.Code, Is.EqualTo("53"));
        }
    }
}
