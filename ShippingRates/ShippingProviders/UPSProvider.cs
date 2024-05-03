using ShippingRates.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ShippingRates.ShippingProviders
{
    public class UPSProvider : AbstractShippingProvider
    {
        public override string Name => "UPS";

        readonly UPSProviderConfiguration _configuration;
        readonly HttpClient _httpClient;

        public UPSProvider(UPSProviderConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if (string.IsNullOrEmpty(_configuration.ClientId))
                throw new Exception("ClientId is required");
            if (string.IsNullOrEmpty(_configuration.ClientSecret))
                throw new Exception("ClientSecret is required");
            if (string.IsNullOrEmpty(_configuration.AccountNumber))
                throw new Exception("AccountNumber is required");
        }

        public UPSProvider(UPSProviderConfiguration configuration, HttpClient httpClient)
            : this(configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public override async Task GetRates()
        {
            var httpClient = _httpClient ?? new HttpClient();

            var token = await UPSOAuthService.GetTokenAsync(_configuration, httpClient, AddError);

            if (_httpClient == null && httpClient != null)
                httpClient.Dispose();
        }
    }
}
