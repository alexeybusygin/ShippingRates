using System;

namespace ShippingRates
{
    /// <summary>
    ///     Summary Name for Rate.
    /// </summary>
    public class Rate
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="Rate" /> class.
        /// </summary>
        /// <param name="provider">The name of the provider responsible for this rate.</param>
        /// <param name="providerCode">The name of the rate.</param>
        /// <param name="name">A Name of the rate.</param>
        /// <param name="totalCharges">The total cost of this rate.</param>
        /// <param name="delivery">The guaranteed date and time of delivery for this rate.</param>
        /// <param name="options">Rate options (Saturday delivery etc.)</param>
        /// <param name="currencyCode">Currency code (USD, EUR, etc), required for DHL</param>
        public Rate(string provider, string providerCode, string name, decimal totalCharges, DateTime delivery, RateOptions options, string currencyCode)
        {
            Provider = provider;
            ProviderCode = providerCode;
            Name = name;
            TotalCharges = totalCharges;
            GuaranteedDelivery = delivery;
            Options = options ?? new RateOptions();
            CurrencyCode = currencyCode;
        }

        /// <summary>
        ///     The guaranteed date and time of delivery for this rate.
        /// </summary>
        public DateTime GuaranteedDelivery { get; set; }
        /// <summary>
        ///     A Name of the rate, as specified by the provider.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        ///     The <see cref="ShippingProviders.IShippingProvider" /> implementation which provided this rate.
        /// </summary>
        public string Provider { get; set; }
        /// <summary>
        ///     The ProviderCode of the rate, as specified by the provider.
        /// </summary>
        public string ProviderCode { get; set; }
        /// <summary>
        ///     The total cost of this rate.
        /// </summary>
        public decimal TotalCharges { get; set; }
        /// <summary>
        ///     Currency code, if applicable
        /// </summary>
        public string CurrencyCode { get; }
        /// <summary>
        ///     Rate options
        /// </summary>
        public RateOptions Options { get; }

        public override string ToString() =>
            $"{Provider}{Environment.NewLine}\t{ProviderCode}{Environment.NewLine}\t{Name}{Environment.NewLine}\t{TotalCharges} {CurrencyCode}{Environment.NewLine}\t{GuaranteedDelivery}{(Options.SaturdayDelivery ? $"{Environment.NewLine}\tSaturday Delivery" : string.Empty)}";
    }
}
