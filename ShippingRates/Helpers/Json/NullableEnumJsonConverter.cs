using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShippingRates.Helpers.Json;

internal sealed class NullableEnumJsonConverter<TEnum> : JsonConverter<TEnum?>
    where TEnum : struct, Enum
{
    public override TEnum? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException(
                $"Expected string or null for enum {typeof(TEnum).Name}, got {reader.TokenType}.");
        }

        var s = reader.GetString();
        if (string.IsNullOrWhiteSpace(s))
        {
            return null;
        }

        if (Enum.TryParse<TEnum>(s, ignoreCase: true, out var value))
        {
            return value;
        }

        return null;
    }

    public override void Write(
        Utf8JsonWriter writer,
        TEnum? value,
        JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value.ToString());
    }
}
