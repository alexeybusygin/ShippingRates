using NUnit.Framework;
using ShippingRates.Models;

namespace ShippingRates.Tests.Units
{
    [TestFixture]
    public class PackageWeightTests
    {
        [Test()]
        public void LbsToKgs()
        {
            var weight1 = new PackageWeight(UnitsSystem.USCustomary, 5);
            Assert.Multiple(() =>
            {
                Assert.That(weight1.Get(), Is.EqualTo(5));
                Assert.That(weight1.Get(UnitsSystem.USCustomary), Is.EqualTo(5));
                Assert.That(weight1.Get(UnitsSystem.Metric), Is.EqualTo(2.26796185m));
                Assert.That(weight1.GetRounded(UnitsSystem.USCustomary), Is.EqualTo(5));
                Assert.That(weight1.GetRounded(UnitsSystem.Metric), Is.EqualTo(3));
            });

            var weight2 = new PackageWeight(UnitsSystem.USCustomary, 0.3m);
            Assert.Multiple(() =>
            {
                Assert.That(weight2.Get(), Is.EqualTo(0.3m));
                Assert.That(weight2.Get(UnitsSystem.USCustomary), Is.EqualTo(0.3m));
                Assert.That(weight2.Get(UnitsSystem.Metric), Is.EqualTo(0.136077711m));
                Assert.That(weight2.GetRounded(UnitsSystem.USCustomary), Is.EqualTo(1));
                Assert.That(weight2.GetRounded(UnitsSystem.Metric), Is.EqualTo(1));
            });
        }

        [Test()]
        public void KgsToLbs()
        {
            var weight1 = new PackageWeight(UnitsSystem.Metric, 5);
            Assert.Multiple(() =>
            {
                Assert.That(weight1.Get(), Is.EqualTo(5));
                Assert.That(weight1.Get(UnitsSystem.USCustomary), Is.EqualTo(11.0231m));
                Assert.That(weight1.Get(UnitsSystem.Metric), Is.EqualTo(5));
                Assert.That(weight1.GetRounded(UnitsSystem.USCustomary), Is.EqualTo(12));
                Assert.That(weight1.GetRounded(UnitsSystem.Metric), Is.EqualTo(5));
            });

            var weight2 = new PackageWeight(UnitsSystem.Metric, 0.5m);
            Assert.Multiple(() =>
            {
                Assert.That(weight2.Get(), Is.EqualTo(0.5m));
                Assert.That(weight2.Get(UnitsSystem.USCustomary), Is.EqualTo(1.10231m));
                Assert.That(weight2.Get(UnitsSystem.Metric), Is.EqualTo(0.5m));
                Assert.That(weight2.GetRounded(UnitsSystem.USCustomary), Is.EqualTo(2));
                Assert.That(weight2.GetRounded(UnitsSystem.Metric), Is.EqualTo(1));
            });
        }
    }
}
