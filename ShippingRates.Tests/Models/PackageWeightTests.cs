using ShippingRates.Models;

namespace ShippingRates.Tests.Models;

[TestFixture]
public class PackageDimensionTests
{
    [Test()]
    public void InchesToCm()
    {
        var dimension1 = new PackageDimension(UnitsSystem.USCustomary, 5);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dimension1.Get(), Is.EqualTo(5));
            Assert.That(dimension1.Get(UnitsSystem.USCustomary), Is.EqualTo(5));
            Assert.That(dimension1.Get(UnitsSystem.Metric), Is.EqualTo(12.7m));
            Assert.That(dimension1.GetRounded(UnitsSystem.USCustomary), Is.EqualTo(5));
            Assert.That(dimension1.GetRounded(UnitsSystem.Metric), Is.EqualTo(13));
        }

        var dimension2 = new PackageDimension(UnitsSystem.USCustomary, 0.4m);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dimension2.Get(), Is.EqualTo(0.4m));
            Assert.That(dimension2.Get(UnitsSystem.USCustomary), Is.EqualTo(0.4m));
            Assert.That(dimension2.Get(UnitsSystem.Metric), Is.EqualTo(1.016m));
            Assert.That(dimension2.GetRounded(UnitsSystem.USCustomary), Is.EqualTo(1));
            Assert.That(dimension2.GetRounded(UnitsSystem.Metric), Is.EqualTo(2));
        }
    }

    [Test()]
    public void CmToInches()
    {
        var dimension1 = new PackageDimension(UnitsSystem.Metric, 6);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dimension1.Get(), Is.EqualTo(6));
            Assert.That(dimension1.Get(UnitsSystem.USCustomary), Is.EqualTo(2.362206m));
            Assert.That(dimension1.Get(UnitsSystem.Metric), Is.EqualTo(6));
            Assert.That(dimension1.GetRounded(UnitsSystem.USCustomary), Is.EqualTo(3));
            Assert.That(dimension1.GetRounded(UnitsSystem.Metric), Is.EqualTo(6));
        }

        var dimension2 = new PackageDimension(UnitsSystem.Metric, 2.6m);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dimension2.Get(), Is.EqualTo(2.6m));
            Assert.That(dimension2.Get(UnitsSystem.USCustomary), Is.EqualTo(1.0236226m));
            Assert.That(dimension2.Get(UnitsSystem.Metric), Is.EqualTo(2.6m));
            Assert.That(dimension2.GetRounded(UnitsSystem.USCustomary), Is.EqualTo(2));
            Assert.That(dimension2.GetRounded(UnitsSystem.Metric), Is.EqualTo(3));
        }
    }
}
