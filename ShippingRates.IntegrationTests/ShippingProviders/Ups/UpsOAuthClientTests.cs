using Microsoft.Extensions.Logging.Abstractions;
using ShippingRates.Models;
using ShippingRates.Services.Ups;
using ShippingRates.ShippingProviders;

namespace ShippingRates.IntegrationTests.ShippingProviders.Ups;

[TestFixture]
public class UpsOAuthClientTests
{
    private UPSProviderConfiguration _configuration;
    private static readonly HttpClient _httpClient = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var config = ConfigHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);

        _configuration = new UPSProviderConfiguration
        {
            AccountNumber = config.UPSAccountNumber,
            ClientId = config.UPSClientId,
            ClientSecret = config.UPSClientSecret,
            UseProduction = config.UPSUseProduction
        };
    }

    [Test]
    public async Task GetToken_Success()
    {
        var logger = NullLogger<UPSProvider>.Instance;
        var oauthClient = new UpsOAuthClient(logger);
        var resultAggregator = new RateResultAggregator("UPS");

        var token = await oauthClient.GetTokenAsync(_configuration, _httpClient, resultAggregator);

        var result = resultAggregator.Build();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(token, Is.Not.Null.And.Not.Empty);
            Assert.That(result.Errors, Is.Empty);
            Assert.That(result.InternalErrors, Is.Empty);
        }
    }

    [Test]
    public async Task GetToken_WrongCredentials()
    {
        var logger = NullLogger<UPSProvider>.Instance;
        var oauthClient = new UpsOAuthClient(logger);
        var resultAggregator = new RateResultAggregator("UPS");
        var wrongConfiguration = new UPSProviderConfiguration
        {
            AccountNumber = _configuration.AccountNumber,
            ClientId = "WRONG",
            ClientSecret = "WRONG",
            UseProduction = _configuration.UseProduction
        };

        var token = await oauthClient.GetTokenAsync(wrongConfiguration, _httpClient, resultAggregator);

        var result = resultAggregator.Build();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(token, Is.Null);
            Assert.That(result.Errors, Is.Not.Empty);
            Assert.That(result.InternalErrors, Is.Empty);
            Assert.That(result.Errors, Has.Exactly(1).Items);
        }

        var error = result.Errors[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(error.Description, Does.Contain("Invalid").IgnoreCase);
            Assert.That(error.Number, Is.EqualTo("10401"));
        }
    }
}
