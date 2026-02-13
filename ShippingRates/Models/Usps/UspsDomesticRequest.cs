using ShippingRates.ShippingProviders.Usps;
using System.Text.Json.Serialization;

namespace ShippingRates.Models.Usps;

internal class UspsDomesticRequest : UspsRequestBase
{
    [JsonPropertyName("destinationZIPCode")]
    public string? DestinationZipCode { get; set; }
    [JsonPropertyName("mailClasses")]
    public UspsMailClass[]? MailClasses { get; set; }
    [JsonPropertyName("hasNonstandardCharacteristics")]
    public bool HasNonstandardCharacteristics { get; set; }
}
