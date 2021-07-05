using NUnit.Framework;
using ShippingRates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingRates.Tests
{
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

            var shipmentNoDocs = new Shipment(from, to, new List<Package>() { package1, package2 });
            var shipmentWithSomeDocs = new Shipment(from, to, new List<Package>() { package1, docsPackage1, package2 });
            var shipmentAllDocs = new Shipment(from, to, new List<Package>() { docsPackage1, docsPackage2 });

            Assert.IsFalse(shipmentNoDocs.HasDocumentsOnly);
            Assert.IsFalse(shipmentWithSomeDocs.HasDocumentsOnly);
            Assert.IsTrue(shipmentAllDocs.HasDocumentsOnly);
        }
    }
}
