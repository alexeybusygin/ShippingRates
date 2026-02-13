using Microsoft.Extensions.Logging.Abstractions;
using ShippingRates.Models;
using ShippingRates.Services.Usps;
using ShippingRates.ShippingProviders;
using ShippingRates.ShippingProviders.Usps;

namespace ShippingRates.IntegrationTests.ShippingProviders.Usps;

[TestFixture]
public class UspsOAuthClientTests
{
    private UspsProviderConfiguration _configuration;
    private static readonly HttpClient _httpClient = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var config = ConfigHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);

        _configuration = new UspsProviderConfiguration
        {
            ClientId = config.USPSClientId,
            ClientSecret = config.USPSClientSecret,
            UseProduction = config.USPSUseProduction
        };
    }

    [Test]
    public async Task GetToken_Success()
    {
        var logger = NullLogger<UPSProvider>.Instance;
        var oauthService = new UspsOAuthClient(logger);
        var resultAggregator = new RateResultAggregator("USPS");

        var token = await oauthService.GetTokenAsync(_configuration, _httpClient, resultAggregator);

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
        var oauthService = new UspsOAuthClient(logger);
        var resultAggregator = new RateResultAggregator("USPS");
        var wrongConfiguration = new UspsProviderConfiguration
        {
            ClientId = "WRONG",
            ClientSecret = "WRONG",
            UseProduction = _configuration.UseProduction
        };

        var token = await oauthService.GetTokenAsync(wrongConfiguration, _httpClient, resultAggregator);

        var result = resultAggregator.Build();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(token, Is.Null);
            Assert.That(result.Errors, Is.Not.Empty);
            Assert.That(result.InternalErrors, Is.Empty);
            Assert.That(result.Errors, Has.Exactly(1).Items);
        }

        var error = result.Errors[0];
        Assert.That(error.Description, Does.Contain("Invalid").IgnoreCase);
    }
}
