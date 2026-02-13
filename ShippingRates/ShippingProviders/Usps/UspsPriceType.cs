using System.Text.Json.Serialization;

namespace ShippingRates.ShippingProviders.Usps;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UspsPriceType
{
    RETAIL,
    COMMERCIAL,
    CONTRACT,
}
