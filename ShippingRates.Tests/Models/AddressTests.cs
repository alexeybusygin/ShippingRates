namespace ShippingRates.Tests.Models;

[TestFixture]
public class AddressTests
{
    [TestCase("US")]
    [TestCase("us")]
    [TestCase(" Us ")]
    [TestCase("PR")]
    [TestCase("pr")]
    public void IsUnitedStatesAddress_ReturnsTrue_ForUsAndTerritoriesIgnoringCase(string countryCode)
    {
        var address = new Address("Annapolis", "MD", "21401", countryCode);

        Assert.That(address.IsUnitedStatesAddress(), Is.True);
    }

    [Test]
    public void IsUnitedStatesAddress_ReturnsFalse_ForNonUsCountryCode()
    {
        var address = new Address("Belgrade", "", "11000", "RS");

        Assert.That(address.IsUnitedStatesAddress(), Is.False);
    }
}
