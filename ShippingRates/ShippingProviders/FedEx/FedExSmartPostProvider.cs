using Microsoft.Extensions.Logging;
using ShippingRates.OpenApi.FedEx.RateTransitTimes;
using System.Collections.Generic;
using System.Net.Http;

namespace ShippingRates.ShippingProviders.FedEx;

/// <summary>
///     Provides SmartPost rates (only) from FedEx REST API.
/// </summary>
public class FedExSmartPostProvider : FedExRateTransmitTimesBaseProvider<FedExSmartPostProvider>
{
    public override string Name { get => "FedExSmartPost"; }

    public FedExSmartPostProvider(FedExProviderConfiguration configuration)
        : base(configuration)
    {
        // SmartPost does not allow insured values
        _allowInsuredValues = false;
    }

    public FedExSmartPostProvider(FedExProviderConfiguration configuration, HttpClient httpClient)
        : base(configuration, httpClient)
    {
        _allowInsuredValues = false;
    }

    public FedExSmartPostProvider(FedExProviderConfiguration configuration, ILogger<FedExSmartPostProvider> logger)
        : base(configuration, logger)
    {
        _allowInsuredValues = false;
    }

    public FedExSmartPostProvider(FedExProviderConfiguration configuration, HttpClient httpClient, ILogger<FedExSmartPostProvider> logger)
        : base(configuration, httpClient, logger)
    {
        _allowInsuredValues = false;
    }

    /// <summary>
    /// Sets the service codes.
    /// </summary>
    protected override Dictionary<string, string> ServiceCodes => new()
    {
        {"SMART_POST", "FedEx Smart Post"}
    };

    /// <summary>
    /// Sets shipment details
    /// </summary>
    /// <param name="request"></param>
    protected sealed override void SetShipmentDetails(Full_Schema_Quote_Rate request)
    {
        SetSmartPostDetails(request);
    }

    /// <summary>
    /// Sets SmartPost details
    /// </summary>
    /// <param name="request"></param>
    private void SetSmartPostDetails(Full_Schema_Quote_Rate request)
    {
        request.RequestedShipment.ServiceType = "SMART_POST";
        request.RequestedShipment.SmartPostInfoDetail = new RequestedShipmentSmartPostInfoDetail
        {
            HubId = _configuration.HubId,
            Indicia = RequestedShipmentSmartPostInfoDetailIndicia.PARCEL_SELECT
        };

        // Handle the various SmartPost Indicia scenarios
        // The ones we should mainly care about are as follows:
        // PRESORTED_STANDARD (less than 1 LB)
        // PARCEL_SELECT (1 LB through 70 LB)

        var weight = request.RequestedShipment.TotalWeight;
        if (weight < 1.0)
            request.RequestedShipment.SmartPostInfoDetail.Indicia = RequestedShipmentSmartPostInfoDetailIndicia.PRESORTED_STANDARD;
    }
}
