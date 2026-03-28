using ShippingRates.Helpers.Json;
using System.Text.Json;

namespace ShippingRates.Tests.Helpers.Json;

[TestFixture]
public class NullableDateTimeConvertorTests
{
    [TestFixture]
    public class NullableDateTimeConverterTests
    {
        private JsonSerializerOptions _options;

        [SetUp]
        public void Setup()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(new NullableDateTimeConverter());
        }

        [Test]
        public void Deserialization_ShouldHandleValidAndSpecialCases()
        {
            using (Assert.EnterMultipleScope())
            {
                var resultNull = JsonSerializer.Deserialize<DateTime?>("null", _options);
                Assert.That(resultNull, Is.Null, "JSON 'null' should be deserialized as null.");

                var resultEmpty = JsonSerializer.Deserialize<DateTime?>("\"\"", _options);
                Assert.That(resultEmpty, Is.Null, "An empty string should be deserialized as null.");

                var jsonDate = "\"2026-03-26T15:00:00Z\"";
                var resultDate = JsonSerializer.Deserialize<DateTime?>(jsonDate, _options);

                Assert.That(resultDate, Is.Not.Null, "A valid date string should not return null.");
                Assert.That(resultDate?.Year, Is.EqualTo(2026), "The year should be 2026.");
                Assert.That(resultDate?.Month, Is.EqualTo(3), "The month should be March.");
            }
        }

        [Test]
        public void Deserialization_InvalidDateValue_ThrowsJsonException()
        {
            var invalidDateJson = "\"not-a-valid-date\"";

            var ex = Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<DateTime?>(invalidDateJson, _options));

            Assert.That(ex.Message, Does.Contain("Invalid date value"), "The exception message should indicate an invalid date.");
        }

        [Test]
        public void Deserialization_UnexpectedToken_ThrowsJsonException()
        {
            var jsonObject = "{}";

            Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<DateTime?>(jsonObject, _options),
                "An unexpected token (Object) should throw a JsonException.");
        }

        [Test]
        public void Write_Always_ThrowsNotImplementedException()
        {
            DateTime? testDate = DateTime.UtcNow;

            Assert.Throws<NotImplementedException>(() =>
                JsonSerializer.Serialize(testDate, _options),
                "The Write method should throw NotImplementedException as defined in the class.");
        }
    }
}
