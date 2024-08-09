using NUnit.Framework;
using ShippingRates.Models;

namespace ShippingRates.Tests.Units
{
    [TestFixture]
    public class PackageKgCmTests
    {
        [Test()]
        public void TestDimensions()
        {
            var packageLbsInches = new Package(10, 20, 30, 40, 50);
            var packageKgCm = new PackageKgCm(10, 20, 30, 40, 50);

            Assert.AreNotEqual(packageKgCm.GetHeight(UnitsSystem.Metric), packageLbsInches.GetHeight(UnitsSystem.Metric));
            Assert.AreNotEqual(packageKgCm.GetLength(UnitsSystem.Metric), packageLbsInches.GetLength(UnitsSystem.Metric));
            Assert.AreNotEqual(packageKgCm.GetWidth(UnitsSystem.USCustomary), packageLbsInches.GetWidth(UnitsSystem.USCustomary));
            Assert.AreNotEqual(packageKgCm.GetHeight(UnitsSystem.USCustomary), packageLbsInches.GetHeight(UnitsSystem.USCustomary));

            Assert.AreEqual(40, packageLbsInches.GetWeight(UnitsSystem.USCustomary));
            Assert.AreEqual(18.1436948m, packageLbsInches.GetWeight(UnitsSystem.Metric));
            Assert.AreEqual(88.1848m, packageKgCm.GetWeight(UnitsSystem.USCustomary));
            Assert.AreEqual(40, packageKgCm.GetWeight(UnitsSystem.Metric));
        }
    }
}
