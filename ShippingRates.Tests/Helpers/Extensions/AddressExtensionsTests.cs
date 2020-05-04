using NUnit.Framework;
using ShippingRates.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShippingRates.Helpers.Extensions.Tests
{
    [TestFixture()]
    public class AddressExtensionsTests
    {
        [Test()]
        public void GetFedExAddressTest()
        {
            var address = new Address(
                " 1084 Layman Court ",
                null,
                " #APT 13S",
                " New York",
                "NY",
                " 10001 ",
                "US");

            var fedExAddress = address.GetFedExAddress();

            Assert.NotNull(fedExAddress);
            Assert.AreEqual(fedExAddress.StreetLines.Length, 2);
            Assert.AreEqual(fedExAddress.StreetLines[0], "1084 Layman Court");
            Assert.AreEqual(fedExAddress.StreetLines[1], "#APT 13S");
            Assert.AreEqual(fedExAddress.City, "New York");
            Assert.AreEqual(fedExAddress.StateOrProvinceCode, "NY");
            Assert.AreEqual(fedExAddress.PostalCode, "10001");
            Assert.AreEqual(fedExAddress.CountryCode, "US");
        }
    }
}
