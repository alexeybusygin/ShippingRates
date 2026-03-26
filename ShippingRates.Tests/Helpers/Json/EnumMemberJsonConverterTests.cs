using Microsoft.Extensions.Options;
using ShippingRates.Helpers.Json;
using System.Runtime.Serialization;
using System.Text.Json;

namespace ShippingRates.Tests.Helpers.Json;

[TestFixture]
public class EnumMemberJsonConverterTests
{
    private enum TestStatus
    {
        [EnumMember(Value = "is_active")]
        Active,
        [EnumMember(Value = "is_archived")]
        Archived,
        Unknown
    }

    [Test]
    public void Deserialization_ValidString_ReturnsCorrectEnum()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<TestStatus>());

        using (Assert.EnterMultipleScope())
        {
            var jsonA = "\"is_active\"";
            var resultA = JsonSerializer.Deserialize<TestStatus>(jsonA, options);
            Assert.That(resultA, Is.EqualTo(TestStatus.Active), "The EnumMember match is incorrect.");

            var jsonB = "\"Unknown\"";
            var resultB = JsonSerializer.Deserialize<TestStatus>(jsonB, options);
            Assert.That(resultB, Is.EqualTo(TestStatus.Unknown), "The name match is incorrect without the attribute.");

            var jsonC = "\"IS_ARCHIVED\"";
            var resultC = JsonSerializer.Deserialize<TestStatus>(jsonC, options);
            Assert.That(resultC, Is.EqualTo(TestStatus.Archived), "The case sensitivity test was failed.");
        }
    }
    [Test]
    public void Deserialization_InvalidString_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<TestStatus>());

        var invalidJson = "\"this_value_does_not_exist\"";

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<TestStatus>(invalidJson, options));
    }

    [Test]
    public void Serialization_EnumMemberValue_ReturnsCorrectString()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<TestStatus>());

        using (Assert.EnterMultipleScope())
        {
            var jsonA = JsonSerializer.Serialize(TestStatus.Active, options);
            Assert.That(jsonA, Is.EqualTo("\"is_active\""), "Serialization: The EnumMember value was incorrectly passed to the JSON.");

            var jsonB = JsonSerializer.Serialize(TestStatus.Unknown, options);
            Assert.That(jsonB, Is.EqualTo("\"Unknown\""), "Serialization: A non-attribute value was incorrectly passed to JSON.");
        }
    }

    [Test]
    public void Deserialization_NonStringToken_ThrowsJsonException()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumMemberJsonConverter<TestStatus>());

        var invalidJson = "123";

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<TestStatus>(invalidJson, options),
            "A JSONException should have been thrown when a numerical value was sent.");
    }
}
