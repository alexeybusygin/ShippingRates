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
            Assert.AreEqual(5, weight1.Get());
            Assert.AreEqual(5, weight1.Get(UnitsSystem.USCustomary));
            Assert.AreEqual(2.26796185m, weight1.Get(UnitsSystem.Metric));
            Assert.AreEqual(5, weight1.GetRounded(UnitsSystem.USCustomary));
            Assert.AreEqual(3, weight1.GetRounded(UnitsSystem.Metric));

            var weight2 = new PackageWeight(UnitsSystem.USCustomary, 0.3m);
            Assert.AreEqual(0.3m, weight2.Get());
            Assert.AreEqual(0.3m, weight2.Get(UnitsSystem.USCustomary));
            Assert.AreEqual(0.136077711m, weight2.Get(UnitsSystem.Metric));
            Assert.AreEqual(1, weight2.GetRounded(UnitsSystem.USCustomary));
            Assert.AreEqual(1, weight2.GetRounded(UnitsSystem.Metric));
        }

        [Test()]
        public void KgsToLbs()
        {
            var weight1 = new PackageWeight(UnitsSystem.Metric, 5);
            Assert.AreEqual(5, weight1.Get());
            Assert.AreEqual(11.0231m, weight1.Get(UnitsSystem.USCustomary));
            Assert.AreEqual(5, weight1.Get(UnitsSystem.Metric));
            Assert.AreEqual(12, weight1.GetRounded(UnitsSystem.USCustomary));
            Assert.AreEqual(5, weight1.GetRounded(UnitsSystem.Metric));

            var weight2 = new PackageWeight(UnitsSystem.Metric, 0.5m);
            Assert.AreEqual(0.5m, weight2.Get());
            Assert.AreEqual(1.10231m, weight2.Get(UnitsSystem.USCustomary));
            Assert.AreEqual(0.5m, weight2.Get(UnitsSystem.Metric));
            Assert.AreEqual(2, weight2.GetRounded(UnitsSystem.USCustomary));
            Assert.AreEqual(1, weight2.GetRounded(UnitsSystem.Metric));
        }
    }
}
