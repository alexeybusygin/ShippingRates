using NUnit.Framework;

namespace ShippingRates.Tests.Units
{
    [TestFixture]
    public class PackageTests
    {
        [Theory]
        [TestCase(3.5, 3, 8)]
        [TestCase(5.8, 5, 12.8)]
        [TestCase(6.2, 6, 3.2)]
        [TestCase(0.21875, 0, 3.5)]
        [TestCase(0.8125, 0, 13)]
        public void PoundsAndOuncesCalculatedCorrectly(decimal weight, int pounds, decimal ounces)
        {
            var package = new Package(1, 2, 3, weight, 100);
            Assert.That(package.PoundsAndOunces.Pounds, Is.EqualTo(pounds));
            Assert.That(package.PoundsAndOunces.Ounces, Is.EqualTo(ounces));
        }
    }
}
