namespace ShippingRates.ShippingProviders
{
    public class UPSProviderConfiguration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AccountNumber { get; set; }
        public bool UseProduction { get; set; }
        public string ServiceDescription { get; set; }
        public bool UseRetailRates { get; set; }
        public bool UseDailyRates { get; set; }
        public bool UseNegotiatedRates { get; set; }
    }
}
