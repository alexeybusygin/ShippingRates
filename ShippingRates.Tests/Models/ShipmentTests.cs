namespace ShippingRates.Tests.Models;

[TestFixture()]
public class ShipmentTests
{
    [Test()]
    public void ShipmentTest_HasDocumentsOnly()
    {
        var from = new Address("Annapolis", "MD", "21401", "US");
        var to = new Address("", "", "30404", "US");

        var package1 = new Package(1, 1, 1, 3, 0);
        var package2 = new Package(1, 2, 3, 4, 5);
        var docsPackage1 = new DocumentsPackage(1, 2);
        var docsPackage2 = new DocumentsPackage(3, 4);

        var shipmentNoDocs = new Shipment(from, to, [package1, package2]);
        var shipmentWithSomeDocs = new Shipment(from, to, [package1, docsPackage1, package2]);
        var shipmentAllDocs = new Shipment(from, to, [docsPackage1, docsPackage2]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(shipmentNoDocs.HasDocumentsOnly, Is.False);
            Assert.That(shipmentWithSomeDocs.HasDocumentsOnly, Is.False);
            Assert.That(shipmentAllDocs.HasDocumentsOnly, Is.True);
        }
    }
}
