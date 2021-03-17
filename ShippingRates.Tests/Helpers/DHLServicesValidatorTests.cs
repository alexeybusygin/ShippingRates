using NUnit.Framework;
using ShippingRates.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShippingRates.Helpers.Tests
{
    [TestFixture()]
    public class DHLServicesValidatorTests
    {
        [Test()]
        public void IsServiceValidTest()
        {
            Assert.IsTrue(DHLServicesValidator.IsServiceValid('M'));
            Assert.IsTrue(DHLServicesValidator.IsServiceValid('n'));
            Assert.IsFalse(DHLServicesValidator.IsServiceValid('9'));
        }

        [Test()]
        public void GetValidServicesTest()
        {
            var services = new char[] { 'M', 'n', '9' };
            var validServices = DHLServicesValidator.GetValidServices(services);

            Assert.IsTrue(validServices.Count() == 2);
            Assert.IsTrue(validServices.Count(s => s == 'M') == 1);
            Assert.IsTrue(validServices.Count(s => s == 'n') == 0);
            Assert.IsTrue(validServices.Count(s => s == 'N') == 1);
            Assert.IsTrue(validServices.Count(s => s == '9') == 0);
        }
    }
}
