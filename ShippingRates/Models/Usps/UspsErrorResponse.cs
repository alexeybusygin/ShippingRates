using System.Text.Json.Serialization;

namespace ShippingRates.Models.Usps
{
    internal class UspsErrorResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }
        [JsonPropertyName("error_description")]
        public string ErrorDescription { get; set; }
        [JsonPropertyName("error_uri")]
        public string ErrorUri { get; set; }
    }
}
