using ShippingRates.Models;
using ShippingRates.Models.Usps;
using ShippingRates.Services.Usps;
using System.Net;
using System.Text.Json;

namespace ShippingRates.Tests.Services.Usps;

[TestFixture]
public class UspsPricesServiceTests
{
    [Test]
    public async Task GetDomesticPrices_ExplicitMailingDate_SerializesDateOnly()
    {
        var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var service = new UspsPricesService(null);
        var request = new UspsDomesticRequest
        {
            MailingDate = new DateTime(2026, 8, 2, 10, 30, 0)
        };

        await service.GetDomesticPrices(
            httpClient,
            "token",
            request,
            false,
            new RateResultAggregator("USPS"));

        using var document = JsonDocument.Parse(handler.RequestContent!);
        Assert.That(
            document.RootElement.GetProperty("mailingDate").GetString(),
            Is.EqualTo("2026-08-02"));
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public string? RequestContent { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestContent = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            };
        }
    }
}
