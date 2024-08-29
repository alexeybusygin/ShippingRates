using NUnit.Framework;
using System.Linq;

namespace ShippingRates.Helpers.Tests
{
    [TestFixture()]
    public class DHLServicesValidatorTests
    {
        [Test()]
        public void IsServiceValidTest()
        {
            Assert.Multiple(() =>
            {
                Assert.That(DHLServicesValidator.IsServiceValid('M'), Is.True);
                Assert.That(DHLServicesValidator.IsServiceValid('n'), Is.True);
                Assert.That(DHLServicesValidator.IsServiceValid('9'), Is.False);
            });
        }

        [Test()]
        public void GetValidServicesTest()
        {
            var services = new char[] { 'M', 'n', '9' };
            var validServices = DHLServicesValidator.GetValidServices(services);

            Assert.Multiple(() =>
            {
                Assert.That(validServices, Has.Length.EqualTo(2));
                Assert.That(validServices.Count(s => s == 'M'), Is.EqualTo(1));
                Assert.That(validServices.Where(s => s == 'n'), Is.Empty);
                Assert.That(validServices.Count(s => s == 'N'), Is.EqualTo(1));
                Assert.That(validServices.Where(s => s == '9'), Is.Empty);
            });
        }
    }
}
