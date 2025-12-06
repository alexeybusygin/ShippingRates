using ShippingRates.ShippingProviders.Usps;
using System.Text.Json.Serialization;

namespace ShippingRates.Models.Usps;

internal class UspsInternationalRequest : UspsRequestBase
{
    [JsonPropertyName("foreignPostalCode")]
    public string? ForeignPostalCode { get; set; }
    [JsonPropertyName("destinationCountryCode")]
    public string? DestinationCountryCode { get; set; }
    [JsonPropertyName("mailClass")]
    public UspsMailClass MailClass { get; set; }
}
