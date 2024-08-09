using System.Text.Json.Serialization;

namespace ShippingRates.Models.UPS
{
    internal class UpsErrorResponse
    {
        [JsonPropertyName("response")]
        public UpsErrorResponseBody Response { get; set; }
    }

    class UpsErrorResponseBody
    {
        [JsonPropertyName("errors")]
        public UpsErrorItem[] Errors { get; set; }
    }

    class UpsErrorItem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
