using ShippingRates.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ShippingRates.ShippingProviders
{
    /// <summary>
    /// DHL Provider Configuration
    /// </summary>
    public class DHLProviderConfiguration
    {
        public string SiteId { get; set; }
        public string Password { get; set; }
        public bool UseProduction { get; set; }
        public string PaymentAccountNumber { get; set; }
        [Obsolete("Timeout property will be ignored in the future versions, pass HttpClient with a necessary timeout instead")]
        public int TimeOut { get; set; } = DHLProvider.DefaultTimeout;

        public IReadOnlyCollection<char> ServicesIncluded { get => new ReadOnlyCollection<char>(_includedServices); }
        public IReadOnlyCollection<char> ServicesExcluded { get => new ReadOnlyCollection<char>(_excludedServices); }

        private readonly List<char> _includedServices = new List<char>();
        private readonly List<char> _excludedServices = new List<char>();

        public DHLProviderConfiguration(string siteId, string password, bool useProduction)
        {
            SiteId = siteId;
            Password = password;
            UseProduction = useProduction;
        }

        public DHLProviderConfiguration IncludeServices(char[] services)
        {
            _includedServices.AddRange(DHLServicesValidator.GetValidServices(services));
            return this;
        }

        public DHLProviderConfiguration ExcludeServices(char[] services)
        {
            _excludedServices.AddRange(DHLServicesValidator.GetValidServices(services));
            return this;
        }
    }
}
