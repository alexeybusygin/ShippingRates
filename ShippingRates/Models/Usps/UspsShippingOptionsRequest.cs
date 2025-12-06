using ShippingRates.ShippingProviders.Usps;
using System;
using System.Text.Json.Serialization;

namespace ShippingRates.Models.Usps;

internal class UspsShippingOptionsRequest
{
    [JsonPropertyName("pricingOptions")]
    public UpspPricingOptions[] PricingOptions { get; set; } = [];
    [JsonPropertyName("originZIPCode")]
    public string? OriginZipCode { get; set; }
    [JsonPropertyName("destinationZIPCode")]
    public string? DestinationZipCode { get; set; }
    [JsonPropertyName("packageDescription")]
    public UspsDomesticPackageDescription PackageDescription { get; set; } = new UspsDomesticPackageDescription();
    [JsonPropertyName("shippingFilter")]
    public string? ShippingFilter { get; set; }
}

internal class UpspPricingOptions
{
    [JsonPropertyName("priceType")]
    public UspsPriceType PriceType { get; set; }
    [JsonPropertyName("paymentAccount")]
    public UspsPaymentAccount? PaymentAccount { get; set; }
}


internal class UspsPaymentAccount
{
    [JsonPropertyName("accountType")]
    public string? AccountType { get; set; }
    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; }
}

internal class UspsDomesticPackageDescription
{
    [JsonPropertyName("weight")]
    public double Weight { get; set; }
    [JsonPropertyName("length")]
    public double Length { get; set; }
    [JsonPropertyName("width")]
    public double Width { get; set; }
    [JsonPropertyName("height")]
    public double Height { get; set; }
    [JsonPropertyName("hasNonstandardCharacteristics")]
    public bool HasNonstandardCharacteristics { get; set; }
    [JsonPropertyName("mailClass")]
    public UspsMailClass MailClass { get; set; }
    [JsonPropertyName("extraServices")]
    public UspsExtraServiceCode[]? ExtraServices { get; set; }
    [JsonPropertyName("itemValue")]
    public double ItemValue { get; set; }
    [JsonPropertyName("mailingDate")]
    public DateTime? MailingDate { get; set; }
}
