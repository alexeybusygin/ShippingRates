using System.Text.Json.Serialization;

namespace ShippingRates.Models.UPS
{
    internal class UPSErrorResponse
    {
        [JsonPropertyName("response")]
        public UPSErrorResponseBody Response { get; set; }
    }

    class UPSErrorResponseBody
    {
        [JsonPropertyName("errors")]
        public UPSErrorItem[] Errors { get; set; }
    }

    class UPSErrorItem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
