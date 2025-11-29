namespace ShippingRates.Models.OAuth
{
    /// <summary>
    /// Configuration settings used for OAuth authentication with a shipping provider.
    /// </summary>
    public interface IOAuthConfiguration
    {
        /// <summary>
        /// The OAuth client identifier issued by the provider.
        /// </summary>
        string ClientId { get; set; }

        /// <summary>
        /// The OAuth client secret used to authenticate the application.
        /// </summary>
        string ClientSecret { get; set; }

        /// <summary>
        /// Indicates whether the client should target production (live) API endpoints.
        /// If <c>false</c>, sandbox or test endpoints are used instead.
        /// </summary>
        bool UseProduction { get; set; }
    }
}
