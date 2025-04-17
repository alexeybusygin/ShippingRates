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
        /// Use retails rates (for shipping from a UPS retail location).
        /// Overrides CustomerClassification.
        /// </summary>
        public bool UseRetailRates { get; set; }
        /// <summary>
        /// Use daily rates (for customers who have a scheduled pickup and/or an account that provides you with daily rates).
        /// Overrides CustomerClassification.
        /// </summary>
        public bool UseDailyRates { get; set; }
        /// <summary>
        /// Use negotiated rates (only shippers approved to ship using negotiated rates can use negotiated rates)
        /// </summary>
        public bool UseNegotiatedRates { get; set; }
        /// <summary>
        /// Customer classification code, valid for shipments originating from a US address. Ignored for non-US shipments.
        /// Default value is 'Rates associated with Shipper Number.'
        /// </summary>
        public UPSCustomerClassification CustomerClassification { get; set; } = UPSCustomerClassification.ShipperNumberRates;
    }

    public enum UPSCustomerClassification
    {
        /// <summary>
        /// Rates associated with Shipper Number
        /// </summary>
        ShipperNumberRates = 0,
        /// <summary>
        /// Daily rates, for customers who have a scheduled pickup and/or an account that provides you with daily rates
        /// </summary>
        DailyRates = 1,
        /// <summary>
        /// Retails rates, for shipping from a UPS retail location
        /// </summary>
        RetailRates = 4,
        RegionalRates = 5,
        GeneralListRates = 6,
        StandardListRates = 53
    }
}
