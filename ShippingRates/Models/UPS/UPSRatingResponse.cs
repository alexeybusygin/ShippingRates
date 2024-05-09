namespace ShippingRates.Models.UPS
{
    internal class UPSRatingResponse
    {
        public RateResponse RateResponse { get; set; }
    }

    internal class RateResponse
    {
        public Response Response { get; set; }
        public RatedShipment[] RatedShipment { get; set; }
    }

    internal class Response
    {
        public ResponseStatus ResponseStatus { get; set; }
    }

    internal class ResponseStatus
    {
        public string Code { get; set; }
    }

    internal class RatedShipment
    {
        public Service Service { get; set; }
        public Charge TotalCharges { get; set; }
        public GuaranteedDelivery GuaranteedDelivery { get; set; }
    }

    internal class Charge
    {
        public string CurrencyCode { get; set; }
        public string MonetaryValue { get; set; }
    }

    internal class GuaranteedDelivery
    {
        public string BusinessDaysInTransit { get; set; }
        public string DeliveryByTime { get; set; }
    }
}
