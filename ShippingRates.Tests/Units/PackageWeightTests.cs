using NUnit.Framework;
using ShippingRates.Models;

namespace ShippingRates.Tests.Units
{
    [TestFixture]
    public class PackageDimensionTests
    {
        [Test()]
        public void InchesToCm()
        {
            var dimension1 = new PackageDimension(UnitsSystem.USCustomary, 5);
            Assert.AreEqual(5, dimension1.Get());
            Assert.AreEqual(5, dimension1.Get(UnitsSystem.USCustomary));
            Assert.AreEqual(12.7m, dimension1.Get(UnitsSystem.Metric));
            Assert.AreEqual(5, dimension1.GetRounded(UnitsSystem.USCustomary));
            Assert.AreEqual(13, dimension1.GetRounded(UnitsSystem.Metric));

            var dimension2 = new PackageDimension(UnitsSystem.USCustomary, 0.4m);
            Assert.AreEqual(0.4m, dimension2.Get());
            Assert.AreEqual(0.4m, dimension2.Get(UnitsSystem.USCustomary));
            Assert.AreEqual(1.016m, dimension2.Get(UnitsSystem.Metric));
            Assert.AreEqual(1, dimension2.GetRounded(UnitsSystem.USCustomary));
            Assert.AreEqual(2, dimension2.GetRounded(UnitsSystem.Metric));
        }

        [Test()]
        public void CmToInches()
        {
            var dimension1 = new PackageDimension(UnitsSystem.Metric, 6);
            Assert.AreEqual(6, dimension1.Get());
            Assert.AreEqual(2.362206m, dimension1.Get(UnitsSystem.USCustomary));
            Assert.AreEqual(6, dimension1.Get(UnitsSystem.Metric));
            Assert.AreEqual(3, dimension1.GetRounded(UnitsSystem.USCustomary));
            Assert.AreEqual(6, dimension1.GetRounded(UnitsSystem.Metric));

            var dimension2 = new PackageDimension(UnitsSystem.Metric, 2.6m);
            Assert.AreEqual(2.6m, dimension2.Get());
            Assert.AreEqual(1.0236226m, dimension2.Get(UnitsSystem.USCustomary));
            Assert.AreEqual(2.6m, dimension2.Get(UnitsSystem.Metric));
            Assert.AreEqual(2, dimension2.GetRounded(UnitsSystem.USCustomary));
            Assert.AreEqual(3, dimension2.GetRounded(UnitsSystem.Metric));
        }
    }
}
