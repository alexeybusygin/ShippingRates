namespace ShippingRates.RateServiceWebReference
{
    public partial class RatePortTypeClient
    {
        /// <summary>
        /// </summary>
        /// <param name="production"></param>
        public RatePortTypeClient(bool production) :
            this(GetDefaultBinding(), new System.ServiceModel.EndpointAddress(production
                ? "https://ws.fedex.com:443/web-services/rate"
                : "https://wsbeta.fedex.com:443/web-services/rate"))
        {
        }
    }
}
