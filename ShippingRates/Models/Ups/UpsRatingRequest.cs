namespace ShippingRates.Models.Ups
{
    internal class UpsRatingRequest
    {
        public RateRequest RateRequest { get; set; }
    }

    internal class RateRequest
    {
        public TransactionReference TransactionReference { get; set; }
        public PickupType PickupType { get; set; }
        public CustomerClassification CustomerClassification { get; set; }
        public UpsShipment Shipment { get; set; }
    }

    internal class TransactionReference
    {
        public string CustomerContext { get; set; }
    }

    internal class PickupType
    {
        public string Code { get; set; }
    }

    internal class CustomerClassification
    {
        public string Code { get; set; }
    }
}
