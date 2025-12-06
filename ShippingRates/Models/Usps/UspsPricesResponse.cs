using ShippingRates.Helpers.Json;
using ShippingRates.ShippingProviders.Usps;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ShippingRates.Models.Usps;

internal class UspsPricesResponse
{
    [JsonPropertyName("rateOptions")]
    public IReadOnlyList<UspsRateOption>? RateOptions { get; set; }
}

internal sealed class UspsRateOption
{
    /// <summary>
    /// The total base price, including the rate, fees and pound postage.
    /// </summary>
    [JsonPropertyName("totalBasePrice")]
    public double TotalBasePrice { get; set; }

    /// <summary>
    /// Detailed rates for this option.
    /// </summary>
    [JsonPropertyName("rates")]
    public IReadOnlyList<UspsRateDetails>? Rates { get; set; }

    /// <summary>
    /// Extra services associated with this rate option (if any).
    /// </summary>
    [JsonPropertyName("extraServices")]
    public IReadOnlyList<UspsExtraRateDetails>? ExtraServices { get; set; }

    /// <summary>
    /// The total price, including totalBasePrice + all extra services.
    /// Returned only when extraServices are passed in the request.
    /// </summary>
    [JsonPropertyName("totalPrice")]
    public double? TotalPrice { get; set; }
}

internal sealed class UspsRateDetails
{
    /// <summary>
    /// Stock keeping unit for the designated rate.
    /// </summary>
    [JsonPropertyName("SKU")]
    public string? Sku { get; set; }

    /// <summary>
    /// Description of the price.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Price type: RETAIL, COMMERCIAL, etc.
    /// </summary>
    [JsonPropertyName("priceType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UspsResponsePriceType PriceType { get; set; }

    /// <summary>
    /// The postage price.
    /// </summary>
    [JsonPropertyName("price")]
    public double Price { get; set; }

    /// <summary>
    /// Calculated package weight. Greater of dimWeight and weight is used.
    /// </summary>
    [JsonPropertyName("weight")]
    public double Weight { get; set; }

    /// <summary>
    /// Calculated dimensional weight.
    /// </summary>
    [JsonPropertyName("dimWeight")]
    public double DimWeight { get; set; }

    /// <summary>
    /// Fees associated with the package.
    /// </summary>
    [JsonPropertyName("fees")]
    public IReadOnlyList<UspsRateFee>? Fees { get; set; }

    /// <summary>
    /// Effective start date of the rate.
    /// </summary>
    [JsonPropertyName("startDate")]
    [JsonConverter(typeof(NullableDateTimeConverter))]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Effective end date of the rate. Blank if none yet.
    /// </summary>
    [JsonPropertyName("endDate")]
    [JsonConverter(typeof(NullableDateTimeConverter))]
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Mail class of the price.
    /// </summary>
    [JsonPropertyName("mailClass")]
    public string? MailClass { get; set; }

    /// <summary>
    /// Indicates the calculated zone between the provided origin and destination postal codes
    /// for a given mail class, mailing date, and weight.
    /// </summary>
    [JsonPropertyName("zone")]
    public string? Zone { get; set; }

    /// <summary>
    /// A business friendly name for a product that can be displayed to a customer on a shipping portal.
    /// </summary>
    [JsonPropertyName("productName")]
    public string? ProductName { get; set; }

    /// <summary>
    /// A business friendly description for a product that can be displayed to a customer on a shipping portal.
    /// </summary>
    [JsonPropertyName("productDefinition")]
    public string? ProductDefinition { get; set; }

    /// <summary>
    /// PProcessing category for the provided rate, this value can be used in the labels API to ensure the provided rate is applied.
    /// </summary>
    [JsonPropertyName("processingCategory")]
    [JsonConverter(typeof(NullableEnumJsonConverter<UspsProcessingCategory>))]
    public UspsProcessingCategory? ProcessingCategory { get; set; }

    /// <summary>
    /// Two-digit rate indicator code.
    /// </summary>
    [JsonPropertyName("rateIndicator")]
    public string? RateIndicator { get; set; }

    /// <summary>
    /// Destination Entry Facility type.
    /// </summary>
    [JsonPropertyName("destinationEntryFacilityType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UspsDestinationEntryFacilityType? DestinationEntryFacilityType { get; set; }
}

internal sealed class UspsRateFee
{
    /// <summary>
    /// Name of the fee.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Pricing SKU for the fee.
    /// </summary>
    [JsonPropertyName("SKU")]
    public string? Sku { get; set; }

    /// <summary>
    /// Price for the fee.
    /// </summary>
    [JsonPropertyName("price")]
    public double Price { get; set; }
}

internal sealed class UspsExtraRateDetails
{
    /// <summary>
    /// SKU for the extra service.
    /// </summary>
    [JsonPropertyName("SKU")]
    public string? Sku { get; set; }

    /// <summary>
    /// Postage rate for the extra service.
    /// </summary>
    [JsonPropertyName("price")]
    public double Price { get; set; }

    /// <summary>
    /// Price type: RETAIL, COMMERCIAL, etc.
    /// </summary>
    [JsonPropertyName("priceType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UspsResponsePriceType PriceType { get; set; }

    /// <summary>
    /// Extra service code (370, 813, 820, etc.).
    /// </summary>
    [JsonPropertyName("extraService")]
    public string? ExtraService { get; set; }

    /// <summary>
    /// Description of the extra service.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// TODO: Handle warnings properly when needed.

    /// <summary>
    /// Any warnings returned for this option.
    /// </summary>
    //[JsonPropertyName("warnings")]
    //public List<string>? Warnings { get; set; }
}

#region Enums

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UspsResponsePriceType
{
    RETAIL,
    COMMERCIAL,
    COMMERCIAL_BASE,
    COMMERCIAL_PLUS,
    CONTRACT
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UspsDestinationEntryFacilityType
{
    NONE,
    AREA_DISTRIBUTION_CENTER,
    AUXILIARY_SERVICE_FACILITY,
    DESTINATION_DELIVERY_UNIT,
    DESTINATION_NETWORK_DISTRIBUTION_CENTER,
    DESTINATION_SECTIONAL_CENTER_FACILITY,
    DESTINATION_SERVICE_HUB,
    INTERNATIONAL_SERVICE_CENTER
}

#endregion
