using ShippingRates.Helpers.Json;
using ShippingRates.ShippingProviders.Usps;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ShippingRates.Models.Usps;

internal class UspsShippingOptionsResponse
{

    [JsonPropertyName("originZIPCode")]
    public string? OriginZipCode { get; set; }
    [JsonPropertyName("destinationZIPCode")]
    public string? DestinationZipCode { get; set; }
    [JsonPropertyName("pricingOptions")]
    public IReadOnlyList<UspsDomesticPricingOption>? PricingOptions { get; set; }
}

internal class UspsDomesticPricingOption
{
    [JsonPropertyName("shippingOptions")]
    public UspsDomesticOptions[]? Options { get; set; }
    [JsonPropertyName("priceType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UspsResponsePriceType PriceType { get; set; }
}

internal class UspsDomesticOptions
{
    [JsonPropertyName("rateOptions")]
    public UspsDomesticRateOptions[]? RateOptions { get; set; }
    [JsonPropertyName("mailClass")]
    public string? MailClass { get; set; }
}

internal class UspsDomesticRateOptions
{
    [JsonPropertyName("commitment")]
    public UspsCommitment? Commitment { get; set; }
    [JsonPropertyName("totalPrice")]
    public double TotalPrice { get; set; }
    [JsonPropertyName("totalBasePrice")]
    public double TotalBasePrice { get; set; }
    [JsonPropertyName("rates")]
    public IReadOnlyList<UspsDomesticRate>? Rates { get; set; }
}

internal class UspsCommitment
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("scheduleDeliveryDate")]
    [JsonConverter(typeof(NullableDateTimeConverter))]
    public DateTime? ScheduleDeliveryDate { get; set; }
    [JsonPropertyName("guaranteedDelivery")]
    public bool GuaranteedDelivery { get; set; }
    [JsonPropertyName("isPriorityMailNextDay")]
    public bool IsPriorityMailNextDay { get; set; }
}

internal class UspsDomesticRate
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("startDate")]
    [JsonConverter(typeof(NullableDateTimeConverter))]
    public DateTime? StartDate { get; set; }
    [JsonPropertyName("endDate")]
    [JsonConverter(typeof(NullableDateTimeConverter))]
    public DateTime? EndDate { get; set; }
    [JsonPropertyName("SKU")]
    public string? Sku { get; set; }
    [JsonPropertyName("price")]
    public double Price { get; set; }
    [JsonPropertyName("zone")]
    public string? Zone { get; set; }
    [JsonPropertyName("weight")]
    public double Weight { get; set; }
    [JsonPropertyName("dimWeight")]
    public double DimWeight { get; set; }
    [JsonPropertyName("priceType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UspsResponsePriceType PriceType { get; set; }
    [JsonPropertyName("fees")]
    public List<UspsRateFee>? Fees { get; set; }
    [JsonPropertyName("productName")]
    public string? ProductName { get; set; }
    [JsonPropertyName("productDefinition")]
    public string? ProductDefinition { get; set; }
    [JsonPropertyName("processingCategory")]
    [JsonConverter(typeof(NullableEnumJsonConverter<UspsProcessingCategory>))]
    public UspsProcessingCategory? ProcessingCategory { get; set; }
    [JsonPropertyName("rateIndicator")]
    public string? RateIndicator { get; set; }
    [JsonPropertyName("destinationEntryFacilityType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UspsDestinationEntryFacilityType? DestinationEntryFacilityType { get; set; }
    [JsonPropertyName("mailClass")]
    public string? MailClass { get; set; }
}
