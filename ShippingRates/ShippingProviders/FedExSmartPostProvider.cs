using ShippingRates.Helpers.Extensions;
using ShippingRates.RateServiceWebReference;
using System.Collections.Generic;

namespace ShippingRates.ShippingProviders
{
    /// <summary>
    ///     Provides SmartPost rates (only) from FedEx (Federal Express).
    /// </summary>
    public class FedExSmartPostProvider : FedExBaseProvider
    {
        public override string Name { get => "FedExSmartPost"; }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="password"></param>
        /// <param name="accountNumber"></param>
        /// <param name="meterNumber"></param>
        /// <param name="hubId">If specified, the FedEx Rate API will only return SmartPost service type rates. Leave empty to get all service types.</param>
        public FedExSmartPostProvider(string key, string password, string accountNumber, string meterNumber, string hubId)
            : this(key, password, accountNumber, meterNumber, hubId, true) { }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="password"></param>
        /// <param name="accountNumber"></param>
        /// <param name="meterNumber"></param>
        /// <param name="hubId">If specified, the FedEx Rate API will only return SmartPost service type rates. Leave empty to get all service types.</param>
        /// <param name="useProduction"></param>
        public FedExSmartPostProvider(string key, string password, string accountNumber, string meterNumber, string hubId, bool useProduction)
            : this(new FedExProviderConfiguration()
            {
                Key = key,
                Password = password,
                AccountNumber = accountNumber,
                MeterNumber = meterNumber,
                UseProduction = useProduction,
                HubId = hubId
            })
        {
        }

        public FedExSmartPostProvider(FedExProviderConfiguration configuration)
            : base(configuration)
        {
            // SmartPost does not allow insured values
            _allowInsuredValues = false;
        }

        /// <summary>
        /// Sets the service codes.
        /// </summary>
        protected override Dictionary<string, string> ServiceCodes => new Dictionary<string, string>
        {
            {"SMART_POST", "FedEx Smart Post"}
        };

        /// <summary>
        /// Sets shipment details
        /// </summary>
        /// <param name="request"></param>
        protected sealed override void SetShipmentDetails(RateRequest request)
        {
            SetOrigin(request);
            SetDestination(request);
            SetPackageLineItems(request);
            SetSmartPostDetails(request);
        }

        /// <summary>
        /// Sets SmartPost details
        /// </summary>
        /// <param name="request"></param>
        private void SetSmartPostDetails(RateRequest request)
        {
            request.RequestedShipment.ServiceType = "SMART_POST";
            request.RequestedShipment.SmartPostDetail = new SmartPostShipmentDetail { HubId = _configuration.HubId, Indicia = SmartPostIndiciaType.PARCEL_SELECT, IndiciaSpecified = true };

            // Handle the various SmartPost Indicia scenarios
            // The ones we should mainly care about are as follows:
            // PRESORTED_STANDARD (less than 1 LB)
            // PARCEL_SELECT (1 LB through 70 LB)

            var weight = request.RequestedShipment.GetTotalWeight();
            if (weight?.Value < 1.0m)
                request.RequestedShipment.SmartPostDetail.Indicia = SmartPostIndiciaType.PRESORTED_STANDARD;
        }
    }
}
