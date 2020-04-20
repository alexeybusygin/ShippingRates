using NUnit.Framework;

namespace ShippingRates.Tests.Units
{
    [TestFixture]
    public class PackageTests
    {
        [Theory]
        [TestCase(3.5, 3, 8)]
        [TestCase(5.8, 5, 13)]
        [TestCase(6.2, 6, 4)]
        public void PoundsAndOuncesCalculatedCorrectly(decimal weight, int pounds, int ounces)
        {
            var package = new Package(1, 2, 3, weight, 100);
            Assert.AreEqual(package.PoundsAndOunces.Pounds, pounds);
            Assert.AreEqual(package.PoundsAndOunces.Ounces, ounces);
        }
    }
}
