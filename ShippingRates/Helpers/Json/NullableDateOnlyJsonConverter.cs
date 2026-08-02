using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShippingRates.Helpers.Json;

internal sealed class NullableDateOnlyJsonConverter : JsonConverter<DateTime?>
{
    private const string DateFormat = "yyyy-MM-dd";

    public override DateTime? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.String
            && DateTime.TryParseExact(
                reader.GetString(),
                DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var value))
        {
            return value;
        }

        throw new JsonException("Expected a date in yyyy-MM-dd format.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTime? value,
        JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value.ToString(DateFormat, CultureInfo.InvariantCulture));
    }
}
