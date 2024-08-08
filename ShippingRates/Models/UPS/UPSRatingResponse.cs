namespace ShippingRates.Models.UPS
{
    internal class UpsRatingResponse
    {
        public RateResponse RateResponse { get; set; }
    }

    internal class UpsSingleRatingResponse
    {
        public SingleRateResponse RateResponse { get; set; }
        public UpsRatingResponse GetRatesResponse()
        {
            return new UpsRatingResponse()
            {
                RateResponse = new RateResponse()
                {
                    Response = RateResponse?.Response,
                    RatedShipment = new[] { RateResponse?.RatedShipment }
                }
            };
        }
    }

    internal class RateResponse
    {
        public Response Response { get; set; }
        public RatedShipment[] RatedShipment { get; set; }
    }

    internal class SingleRateResponse
    {
        public Response Response { get; set; }
        public RatedShipment RatedShipment { get; set; }
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
        public NegotiatedRateCharges NegotiatedRateCharges { get; set; }
        public GuaranteedDelivery GuaranteedDelivery { get; set; }
    }

    internal class NegotiatedRateCharges
    {
        public Charge TotalCharge { get; set; }
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
