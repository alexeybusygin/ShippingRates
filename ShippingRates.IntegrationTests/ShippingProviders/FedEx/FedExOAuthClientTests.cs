using Microsoft.Extensions.Logging.Abstractions;
using ShippingRates.Models;
using ShippingRates.ShippingProviders;
using ShippingRates.ShippingProviders.FedEx;

namespace ShippingRates.IntegrationTests.ShippingProviders.FedEx;

[TestFixture]
public class FedExOAuthClientTests
{
    private FedExProviderConfiguration _configuration;
    private static readonly HttpClient _httpClient = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var config = ConfigHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);

        _configuration = new FedExProviderConfiguration
        {
            ClientId = config.FedExRestClientId,
            ClientSecret = config.FedExRestClientSecret,
            UseProduction = config.FedExRestUseProduction
        };
    }

    [Test]
    public async Task GetToken_Success()
    {
        var logger = NullLogger<FedExProvider>.Instance;
        var oauthClient = new FedExOAuthClient(logger);
        var resultAggregator = new RateResultAggregator("FedEx");

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
        var logger = NullLogger<FedExProvider>.Instance;
        var oauthClient = new FedExOAuthClient(logger);
        var resultAggregator = new RateResultAggregator("FedEx");
        var wrongConfiguration = new FedExProviderConfiguration
        {
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
            Assert.That(error.Description, Does.Contain("not valid").IgnoreCase);
            Assert.That(error.Number, Is.EqualTo("NOT.AUTHORIZED.ERROR"));
        }
    }
}
