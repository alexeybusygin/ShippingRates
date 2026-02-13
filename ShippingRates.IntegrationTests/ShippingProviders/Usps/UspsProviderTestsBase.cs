using ShippingRates.ShippingProviders.Usps;

namespace ShippingRates.IntegrationTests.ShippingProviders.Usps
{
    public abstract class UspsProviderTestsBase
    {
        private string? UspsClientId;
        private string? UspsClientSecret;

        protected static readonly HttpClient _httpClient = new();

        protected void ConfigSetUp()
        {
            var config = ConfigHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);

            UspsClientId = config.USPSClientId ?? throw new ArgumentException(nameof(config.USPSClientId));
            UspsClientSecret = config.USPSClientSecret ?? throw new ArgumentException(nameof(config.USPSClientSecret));
        }

        protected RateManager GetRateManagerWithUspsProvider(UspsProviderConfiguration? configuration = null)
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new UspsProvider(configuration ?? GetConfiguration(), _httpClient));
            return rateManager;
        }

        protected UspsProviderConfiguration GetConfiguration(UspsMailClass mailClasses)
            => GetConfiguration([mailClasses]);

        protected UspsProviderConfiguration GetConfiguration(UspsMailClass[]? mailClasses = null)
        {
            return new UspsProviderConfiguration()
            {
                ClientId = UspsClientId,
                ClientSecret = UspsClientSecret,
                MailClasses = mailClasses ?? [UspsMailClass.All],
            };
        }
    }
}
