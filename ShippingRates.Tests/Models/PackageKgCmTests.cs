namespace ShippingRates.Tests.Models;

[TestFixture]
public class PackageKgCmTests
{
    [Test()]
    public void TestDimensions()
    {
        var packageLbsInches = new Package(10, 20, 30, 40, 50);
        var packageKgCm = new PackageKgCm(10, 20, 30, 40, 50);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(packageLbsInches.GetHeight(UnitsSystem.Metric), Is.Not.EqualTo(packageKgCm.GetHeight(UnitsSystem.Metric)));
            Assert.That(packageLbsInches.GetLength(UnitsSystem.Metric), Is.Not.EqualTo(packageKgCm.GetLength(UnitsSystem.Metric)));
            Assert.That(packageLbsInches.GetWidth(UnitsSystem.USCustomary), Is.Not.EqualTo(packageKgCm.GetWidth(UnitsSystem.USCustomary)));
            Assert.That(packageLbsInches.GetHeight(UnitsSystem.USCustomary), Is.Not.EqualTo(packageKgCm.GetHeight(UnitsSystem.USCustomary)));

            Assert.That(packageLbsInches.GetWeight(UnitsSystem.USCustomary), Is.EqualTo(40));
            Assert.That(packageLbsInches.GetWeight(UnitsSystem.Metric), Is.EqualTo(18.1436948m));
            Assert.That(packageKgCm.GetWeight(UnitsSystem.USCustomary), Is.EqualTo(88.1848m));
            Assert.That(packageKgCm.GetWeight(UnitsSystem.Metric), Is.EqualTo(40));
        }
    }
}
