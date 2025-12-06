using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShippingRates.Helpers.Json;

internal sealed class EnumMemberJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    private static readonly Dictionary<TEnum, string> _toString;
    private static readonly Dictionary<string, TEnum> _fromString;

    static EnumMemberJsonConverter()
    {
        _toString = [];
        _fromString = new Dictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase);

        var type = typeof(TEnum);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

        foreach (var field in fields)
        {
            var enumValue = (TEnum)field.GetValue(null)!;

            var enumMemberAttr = field
                .GetCustomAttributes(typeof(EnumMemberAttribute), inherit: false)
                .Cast<EnumMemberAttribute>()
                .FirstOrDefault();

            var stringValue = enumMemberAttr?.Value ?? field.Name;

            _toString[enumValue] = stringValue;
            _fromString[stringValue] = enumValue;
        }
    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string for enum {typeof(TEnum).Name}.");
        }

        var s = reader.GetString() ?? throw new JsonException($"Null value for enum {typeof(TEnum).Name}.");

        if (_fromString.TryGetValue(s, out var value))
        {
            return value;
        }

        throw new JsonException($"Unknown value '{s}' for enum {typeof(TEnum).Name}.");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (_toString.TryGetValue(value, out var s))
        {
            writer.WriteStringValue(s);
            return;
        }

        writer.WriteStringValue(value.ToString());
    }
}
