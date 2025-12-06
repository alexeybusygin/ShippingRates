namespace ShippingRates.Models.Ups
{
    internal class UpsShipment
    {
        public Shipper Shipper { get; set; }
        public ShipAddress ShipFrom { get; set; }
        public ShipAddress ShipTo { get; set; }
        public PaymentDetails PaymentDetails { get; set; }
        public Service Service { get; set; }
        public int NumOfPieces { get; set; }
        public string DocumentsOnlyIndicator { get; set; }
        public UpsPackage[] Package { get; set; }
        public ShipmentServiceOptions ShipmentServiceOptions { get; set; }
        public ShipmentRatingOptions ShipmentRatingOptions { get; set; }
        public DeliveryTimeInformation DeliveryTimeInformation { get; set; }
    }
    internal class Shipper
    {
        public string Name { get; set; }
        public string ShipperNumber { get; set; }
        public UpsAddress Address { get; set; }
    }

    internal class ShipAddress
    {
        public string Name { get; set; }
        public UpsAddress Address { get; set; }

        public ShipAddress(UpsAddress address)
        {
            Address = address;
        }
    }

    internal class Service
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }

    internal class PaymentDetails
    {
        public ShipmentCharge ShipmentCharge { get; set; }
    }

    internal class ShipmentCharge
    {
        public string Type { get; set; }
        public BillShipper BillShipper { get; set; }
    }

    internal class BillShipper
    {
        public string AccountNumber { get; set; }
    }

    internal class ShipmentServiceOptions
    {
        public string SaturdayDeliveryIndicator { get; set; }
    }

    internal class ShipmentRatingOptions
    {
        public string NegotiatedRatesIndicator { get; set; }
    }

    internal class DeliveryTimeInformation
    {
        public string PackageBillType { get; set; }
        public Pickup Pickup { get; set; }
    }

    internal class Pickup
    {
        public string Date { get; set; }
        public string Time { get; set; }
    }
}
