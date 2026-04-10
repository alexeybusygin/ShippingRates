using ShippingRates.ShippingProviders;

namespace ShippingRates.Tests
{
    [TestFixture]
    public class RateManagerTests
    {
        private Address _origin;
        private Address _destination;
        private Package _package;

        [SetUp]
        public void SetUp()
        {
            _origin = new Address("City", "State", "12345", "US");
            _destination = new Address("City", "State", "54321", "US");
            _package = new Package(10, 10, 10, 5, 100);
        }

        [Test]
        public async Task GetRatesAsync_AggregatesRatesFromMultipleProviders()
        {
            var rateManager = new RateManager();

            var rate1 = new Rate("Provider 1", "P1", "Service 1", 10m, DateTime.Now, new RateOptions(), "USD");
            var rate2 = new Rate("Provider 2", "P2", "Service 2", 20m, DateTime.Now, new RateOptions(), "USD");

            var provider1 = new FakeShippingProvider("Provider 1", [rate1]);
            var provider2 = new FakeShippingProvider("Provider 2", [rate2]);

            rateManager.AddProvider(provider1);
            rateManager.AddProvider(provider2);

            var shipment = await rateManager.GetRatesAsync(_origin, _destination, _package);

            Assert.That(shipment.Rates, Has.Count.EqualTo(2));

            Assert.That(shipment.Rates.Any(r => r.Provider == "Provider 1" && r.TotalCharges == 10m), Is.True);
            Assert.That(shipment.Rates.Any(r => r.Provider == "Provider 2" && r.TotalCharges == 20m), Is.True);

            Assert.That(shipment.Errors, Is.Empty);
            Assert.That(shipment.InternalErrors, Is.Empty);
        }

        [Test]
        public async Task GetRatesAsync_AggregatesErrorsAndInternalErrors()
        {
            var rateManager = new RateManager();

            var provider1 = new FakeShippingProvider("Provider 1")
            {
                Errors = [new Error { Number = "E1" }],
                InternalErrors = ["IE 1"]
            };

            var provider2 = new FakeShippingProvider("Provider 2")
            {
                Errors = [new Error { Number = "E2" }],
                InternalErrors = ["IE 2"]
            };

            rateManager.AddProvider(provider1);
            rateManager.AddProvider(provider2);

            var shipment = await rateManager.GetRatesAsync(_origin, _destination, _package);

            Assert.That(shipment.Errors, Has.Count.EqualTo(2));
            Assert.That(shipment.InternalErrors, Has.Count.EqualTo(2));

            Assert.That(shipment.Errors.Any(e => e.Number == "E1"), Is.True);
            Assert.That(shipment.Errors.Any(e => e.Number == "E2"), Is.True);

            Assert.That(shipment.InternalErrors.Contains("IE 1"), Is.True);
            Assert.That(shipment.InternalErrors.Contains("IE 2"), Is.True);
        }

        [Test]
        public async Task GetRatesAsync_AppliesAdjustersInOrder()
        {
            var rateManager = new RateManager();

            var rate = new Rate("Provider 1", "P1", "Service 1", 100m, DateTime.Now, new RateOptions(), "USD");

            var provider = new FakeShippingProvider("Provider 1", [rate]);

            rateManager.AddProvider(provider);
            rateManager.AddRateAdjuster(new PercentageAdjuster(5));
            rateManager.AddRateAdjuster(new FixedAmountAdjuster(-5));

            var shipment = await rateManager.GetRatesAsync(_origin, _destination, _package);

            Assert.That(shipment.Rates, Has.Count.EqualTo(1));
            Assert.That(shipment.Rates[0].TotalCharges, Is.EqualTo(100m));

            rateManager = new RateManager();

            rateManager.AddProvider(provider);
            rateManager.AddRateAdjuster(new FixedAmountAdjuster(-5));
            rateManager.AddRateAdjuster(new PercentageAdjuster(5));

            shipment = await rateManager.GetRatesAsync(_origin, _destination, _package);

            Assert.That(shipment.Rates, Has.Count.EqualTo(1));
            Assert.That(shipment.Rates[0].TotalCharges, Is.EqualTo(99.75m));
        }

        [Test]
        public void GetRatesAsync_ThrowsIfCancelled()
        {
            var rateManager = new RateManager();

            var provider = new FakeShippingProvider("Provider 1");

            rateManager.AddProvider(provider);

            var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.ThrowsAsync<OperationCanceledException>(
                async () => await rateManager.GetRatesAsync(_origin, _destination, _package, cancellationToken: cts.Token)
            );

            Assert.That(provider.WasCalled, Is.False);
        }

        private class FakeShippingProvider : IShippingProvider
        {
            public string Name { get; }
            public List<Rate> Rates { get; set; } = [];
            public List<Error> Errors { get; set; } = [];
            public List<string> InternalErrors { get; set; } = [];
            public bool WasCalled { get; private set; }

            public FakeShippingProvider(string name, List<Rate>? rates = null)
            {
                Name = name;
                if (rates != null) Rates = rates;
            }

            public Task<RateResult> GetRatesAsync(Shipment shipment, CancellationToken cancellationToken = default)
            {
                var result = new RateResult();
                result.Rates.AddRange(Rates);
                result.Errors.AddRange(Errors);
                result.InternalErrors.AddRange(InternalErrors);
                WasCalled = true;
                return Task.FromResult(result);
            }
        }

        private class PercentageAdjuster : IRateAdjuster
        {
            private readonly decimal _percentage;

            public PercentageAdjuster(decimal percentage)
            {
                _percentage = percentage;
            }

            public Rate AdjustRate(Rate rate)
            {
                rate.TotalCharges *= 1 + _percentage / 100m;
                return rate;
            }
        }

        private class FixedAmountAdjuster : IRateAdjuster
        {
            private readonly decimal _amount;

            public FixedAmountAdjuster(decimal amount)
            {
                _amount = amount;
            }

            public Rate AdjustRate(Rate rate)
            {
                rate.TotalCharges += _amount;
                return rate;
            }
        }
    }
}