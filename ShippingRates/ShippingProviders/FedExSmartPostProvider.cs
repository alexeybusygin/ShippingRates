using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ShippingRates.Helpers.Extensions;
using ShippingRates.RateServiceWebReference;

namespace ShippingRates.ShippingProviders
{
    /// <summary>
    ///     Provides SmartPost rates (only) from FedEx (Federal Express).
    /// </summary>
    public class FedExSmartPostProvider : FedExBaseProvider
    {
        /// <summary>
        /// If not using the production Rate API, you can use 5531 as the HubID per FedEx documentation.
        /// </summary>
        private string _hubId;

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="password"></param>
        /// <param name="accountNumber"></param>
        /// <param name="meterNumber"></param>
        /// <param name="hubId">If specified, the FedEx Rate API will only return SmartPost service type rates. Leave empty to get all service types.</param>
        public FedExSmartPostProvider(string key, string password, string accountNumber, string meterNumber, string hubId)
        {
            Init(key, password, accountNumber, meterNumber, true, hubId);
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="password"></param>
        /// <param name="accountNumber"></param>
        /// <param name="meterNumber"></param>
        /// <param name="hubId">If specified, the FedEx Rate API will only return SmartPost service type rates. Leave empty to get all service types.</param>
        /// <param name="useProduction"></param>
        public FedExSmartPostProvider(string key, string password, string accountNumber, string meterNumber, string hubId, bool useProduction)
        {
            Init(key, password, accountNumber, meterNumber, useProduction, hubId);
        }

        private void Init(string key, string password, string accountNumber, string meterNumber, bool useProduction, string hubId)
        {
            Name = "FedExSmartPost";

            // SmartPost does not allow insured values
            _allowInsuredValues = false;

            _key = key;
            _password = password;
            _accountNumber = accountNumber;
            _meterNumber = meterNumber;
            _useProduction = useProduction;
            _hubId = hubId;

            SetServiceCodes();
        }

        /// <summary>
        /// Sets the service codes.
        /// </summary>
        protected sealed override void SetServiceCodes()
        {
            _serviceCodes = new Dictionary<string, string>
            {
                {"SMART_POST", "FedEx Smart Post"}
            };
        }

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
            request.RequestedShipment.SmartPostDetail = new SmartPostShipmentDetail { HubId = _hubId, Indicia = SmartPostIndiciaType.PARCEL_SELECT, IndiciaSpecified = true };

            // Handle the various SmartPost Incidia scenarios
            // The ones we should mainly care about are as follows:
            // PRESORTED_STANDARD (less than 1 LB)
            // PARCEL_SELECT (1 LB through 70 LB)

            var weight = request.RequestedShipment.GetTotalWeight();
            if (weight?.Value < 1.0m)
                request.RequestedShipment.SmartPostDetail.Indicia = SmartPostIndiciaType.PRESORTED_STANDARD;
        }
    }
}
