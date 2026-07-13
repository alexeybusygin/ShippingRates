using ShippingRates.Models.Usps;
using System.Text.Json;

namespace ShippingRates.Tests.Models.Usps;

/// <summary>
/// USPS's published Domestic Prices spec defines <c>priceType</c> as a fixed enum
/// (RETAIL, COMMERCIAL, COMMERCIAL_BASE, COMMERCIAL_PLUS, CONTRACT, NSA), but the
/// live API sometimes returns out-of-spec tokens such as <c>"N"</c> — an apparent
/// USPS defect. A strict
/// <see cref="System.Text.Json.Serialization.JsonStringEnumConverter"/> throws on
/// those tokens, which aborts deserialization of the whole response and drops every rate.
/// These tests verify documented values still parse to a real member, while out-of-spec
/// tokens map to null so the rate is preserved instead of throwing.
/// </summary>
[TestFixture]
public class UspsResponsePriceTypeDeserializationTests
{
    [Test]
    public void GetDomesticPrices_WithUnknownPriceType_DoesNotThrowAndKeepsRates()
    {
        // "N" is an undocumented marketing-mail token that is not a member of
        // UspsResponsePriceType.
        const string json = """
        {
            "rateOptions": [
                {
                    "totalBasePrice": 8.10,
                    "rates": [
                        {
                            "SKU": "DUGA0",
                            "description": "USPS Ground Advantage",
                            "priceType": "COMMERCIAL",
                            "price": 8.10,
                            "weight": 5.0,
                            "mailClass": "USPS_GROUND_ADVANTAGE"
                        },
                        {
                            "SKU": "NPGA0",
                            "description": "USPS Marketing Mail Nonprofit",
                            "priceType": "N",
                            "price": 7.55,
                            "weight": 5.0,
                            "destinationEntryFacilityType": "SOME_NEW_FACILITY",
                            "mailClass": "USPS_GROUND_ADVANTAGE"
                        }
                    ]
                }
            ]
        }
        """;

        UspsPricesResponse? response = null;
        Assert.DoesNotThrow(() =>
            response = JsonSerializer.Deserialize<UspsPricesResponse>(json));

        Assert.That(response, Is.Not.Null);
        var rates = response!.RateOptions![0].Rates!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(rates, Has.Count.EqualTo(2), "All rates should survive deserialization.");
            Assert.That(rates[0].PriceType, Is.EqualTo(UspsResponsePriceType.COMMERCIAL), "Known price types still parse.");
            Assert.That(rates[1].PriceType, Is.Null, "Undocumented price type 'N' should deserialize to null.");
            Assert.That(rates[1].DestinationEntryFacilityType, Is.Null, "Unknown facility type should deserialize to null.");
            Assert.That(rates[1].Price, Is.EqualTo(7.55), "The rest of the rate should still be populated.");
        }
    }

    [Test]
    public void GetDomesticShippingOptions_WithUnknownPriceType_DoesNotThrowAndKeepsRates()
    {
        const string json = """
        {
            "originZIPCode": "38732",
            "destinationZIPCode": "98052",
            "pricingOptions": [
                {
                    "priceType": "N",
                    "shippingOptions": [
                        {
                            "mailClass": "USPS_GROUND_ADVANTAGE",
                            "rateOptions": [
                                {
                                    "totalPrice": 7.55,
                                    "totalBasePrice": 7.55,
                                    "rates": [
                                        {
                                            "description": "USPS Marketing Mail Nonprofit",
                                            "SKU": "NPGA0",
                                            "price": 7.55,
                                            "priceType": "N"
                                        }
                                    ]
                                }
                            ]
                        }
                    ]
                }
            ]
        }
        """;

        UspsShippingOptionsResponse? response = null;
        Assert.DoesNotThrow(() =>
            response = JsonSerializer.Deserialize<UspsShippingOptionsResponse>(json));

        Assert.That(response, Is.Not.Null);
        var pricingOption = response!.PricingOptions![0];
        var rate = pricingOption.Options![0].RateOptions![0].Rates![0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(pricingOption.PriceType, Is.Null, "Unknown option price type 'N' should deserialize to null.");
            Assert.That(rate.PriceType, Is.Null, "Unknown rate price type 'N' should deserialize to null.");
            Assert.That(rate.Price, Is.EqualTo(7.55), "The rest of the rate should still be populated.");
        }
    }
}
