using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ShippingRates.Models.Usps;

public sealed class UspsErrorResponse
{
    [JsonPropertyName("apiVersion")]
    public string? ApiVersion { get; set; }

    [JsonPropertyName("error")]
    public UspsError? Error { get; set; }
}

public sealed class UspsError
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("errors")]
    public IReadOnlyList<UspsErrorDetail>? Errors { get; set; }
}

public sealed class UspsErrorDetail
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    [JsonPropertyName("source")]
    public UspsErrorSource? Source { get; set; }
}

public sealed class UspsErrorSource
{
    [JsonPropertyName("parameter")]
    public string? Parameter { get; set; }

    [JsonPropertyName("example")]
    public string? Example { get; set; }
}
