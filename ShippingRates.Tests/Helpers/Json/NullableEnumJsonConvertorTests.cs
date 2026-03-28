using ShippingRates.Helpers.Json;
using System.Text.Json;

namespace ShippingRates.Tests.Helpers.Json;

[TestFixture]
public class NullableEnumJsonConvertorTests
{
    private enum TestStatus
    {
        Active,
        Pending
    }

    private JsonSerializerOptions _options;

    [SetUp]
    public void Setup()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new NullableEnumJsonConverter<TestStatus>());
    }
    [Test]
    public void Deserialization_ShouldHandleAllValidAndSpecialCases()
    {
        using (Assert.EnterMultipleScope())
        {
            var resultNull = JsonSerializer.Deserialize<TestStatus?>("null", _options);
            Assert.That(resultNull, Is.Null, "JSON 'null' should be deserialized as null.");

            var resultEmpty = JsonSerializer.Deserialize<TestStatus?>("\"  \"", _options);
            Assert.That(resultEmpty, Is.Null, "An empty or whitespace string should be deserialized as null.");

            var resultValid = JsonSerializer.Deserialize<TestStatus?>("\"Active\"", _options);
            Assert.That(resultValid, Is.EqualTo(TestStatus.Active), "A valid enum string should be correctly deserialized.");

            var resultUnknown = JsonSerializer.Deserialize<TestStatus?>("\"NonExistentValue\"", _options);
            Assert.That(resultUnknown, Is.Null, "An unknown enum string should be deserialized as null.");
        }
    }

    [Test]
    public void Deserialization_InvalidTokenType_ThrowsJsonException()
    {
        var jsonNumber = "123";

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<TestStatus?>(jsonNumber, _options),
            "A JsonException should be thrown when the JSON token is not a string.");
    }

    [Test]
    public void Serialization_ShouldWriteCorrectValues()
    {
        using (Assert.EnterMultipleScope())
        {
            var jsonValue = JsonSerializer.Serialize((TestStatus?)TestStatus.Active, _options);
            Assert.That(jsonValue, Is.EqualTo("\"Active\""), "The enum value should be serialized as its string name.");

            var jsonNull = JsonSerializer.Serialize((TestStatus?)null, _options);
            Assert.That(jsonNull, Is.EqualTo("null"), "A null enum value should be serialized as 'null'.");
        }
    }
}
