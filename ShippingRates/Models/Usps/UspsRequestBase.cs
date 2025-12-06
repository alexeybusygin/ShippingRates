using ShippingRates.ShippingProviders.Usps;
using System;
using System.Text.Json.Serialization;

namespace ShippingRates.Models.Usps;

internal class UspsRequestBase
{
    [JsonPropertyName("originZIPCode")]
    public string? OriginZipCode { get; set; }
    [JsonPropertyName("weight")]
    public double Weight { get; set; }
    [JsonPropertyName("mailingDate")]
    public DateTime? MailingDate { get; set; }
    [JsonPropertyName("length")]
    public double Length { get; set; }
    [JsonPropertyName("width")]
    public double Width { get; set; }
    [JsonPropertyName("height")]
    public double Height { get; set; }
    [JsonPropertyName("priceType")]
    public UspsPriceType PriceType { get; set; }
    [JsonPropertyName("accountType")]
    public string? AccountType { get; set; }
    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; }
    [JsonPropertyName("itemValue")]
    public double ItemValue { get; set; }
    [JsonPropertyName("extraServices")]
    public UspsExtraServiceCode[]? ExtraServices { get; set; }
}
