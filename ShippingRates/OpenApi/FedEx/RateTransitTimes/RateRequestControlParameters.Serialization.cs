namespace ShippingRates.OpenApi.FedEx.RateTransitTimes
{
    public partial class RateRequestControlParameters
    {
        internal bool SerializeServicesNeededOnRateFailure { get; set; }
        internal bool SerializeVariableOptions { get; set; }
        internal bool SerializeRateSortOrder { get; set; }

        public bool ShouldSerializeServicesNeededOnRateFailure()
        {
            return SerializeServicesNeededOnRateFailure;
        }

        public bool ShouldSerializeVariableOptions()
        {
            return SerializeVariableOptions;
        }

        public bool ShouldSerializeRateSortOrder()
        {
            return SerializeRateSortOrder;
        }
    }
}
