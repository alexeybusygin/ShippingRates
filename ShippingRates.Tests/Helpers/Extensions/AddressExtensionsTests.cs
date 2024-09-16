using NUnit.Framework;
using ShippingRates.Helpers.Extensions;

namespace ShippingRates.Tests.Helpers.Extensions
{
    [TestFixture()]
    public class AddressExtensionsTests
    {
        [Test()]
        public void GetFedExAddressTest()
        {
            var address = new Address(
                line1: " 1084 Layman Court ",
                line2: null,
                line3: " #APT 13S",
                city: " New York",
                state: "NY",
                postalCode: " 10001 ",
                countryCode: "US");

            var fedExAddress = address.GetFedExAddress();
            Assert.That(fedExAddress, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(fedExAddress.StreetLines, Has.Length.EqualTo(2));
                Assert.That(fedExAddress.StreetLines[0], Is.EqualTo("1084 Layman Court"));
                Assert.That(fedExAddress.StreetLines[1], Is.EqualTo("#APT 13S"));
                Assert.That(fedExAddress.City, Is.EqualTo("New York"));
                Assert.That(fedExAddress.StateOrProvinceCode, Is.EqualTo("NY"));
                Assert.That(fedExAddress.PostalCode, Is.EqualTo("10001"));
                Assert.That(fedExAddress.CountryCode, Is.EqualTo("US"));
            });
        }
    }
}
