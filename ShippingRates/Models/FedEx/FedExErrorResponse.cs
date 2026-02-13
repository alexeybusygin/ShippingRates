using System.Text.Json.Serialization;

namespace ShippingRates.Models.FedEx
{
    class FedExErrorResponse
    {
        [JsonPropertyName("errors")]
        public FedExErrorItem[]? Errors { get; set; }
    }

    class FedExErrorItem
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
