using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShippingRates.Helpers.Json;

internal sealed class NullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();

            if (string.IsNullOrEmpty(s))
                return null;

            if (DateTime.TryParse(
                s,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var dt))
            {
                return dt;
            }

            throw new JsonException($"Invalid date value: '{s}'.");
        }

        throw new JsonException($"Unexpected token parsing DateTime?: {reader.TokenType}.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTime? value,
        JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
