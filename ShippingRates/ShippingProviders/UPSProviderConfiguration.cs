namespace ShippingRates.ShippingProviders
{
    /// <summary>
    /// UPS Provider Configuration
    /// </summary>
    public class UPSProviderConfiguration
    {
        /// <summary>
        /// UPS Client Id (required)
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// UPS Client Secret (required)
        /// </summary>
        public string ClientSecret { get; set; }
        /// <summary>
        /// UPS Account Number (6 chars, required)
        /// </summary>
        public string AccountNumber { get; set; }
        /// <summary>
        /// Use production endpoint
        /// </summary>
        public bool UseProduction { get; set; }
        /// <summary>
        /// Optional. Service description from GetServiceCodes() values, e.g. "UPS Second Day Air".
        /// If omitted, all possible services will be fetched
        /// </summary>
        public string ServiceDescription { get; set; }
        /// <summary>
        /// Use retails rates (for shipping from a UPS retail location)
        /// </summary>
        public bool UseRetailRates { get; set; }
        /// <summary>
        /// Use daily rates (for customers who have a scheduled pickup and/or an account that provides you with daily rates)
        /// </summary>
        public bool UseDailyRates { get; set; }
        /// <summary>
        /// Use negotiated rates (only shippers approved to ship using negotiated rates can use negotiated rates)
        /// </summary>
        public bool UseNegotiatedRates { get; set; }
    }
}
