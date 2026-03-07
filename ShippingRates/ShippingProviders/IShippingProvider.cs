using System.Threading.Tasks;
using System.Threading;

namespace ShippingRates.ShippingProviders
{
    /// <summary>
    ///     Defines a standard interface for all shipping providers.
    /// </summary>
    public interface IShippingProvider
    {
        /// <summary>
        ///     The name of the provider.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Retrieves rates from the provider.
        /// </summary>
        Task<RateResult> GetRatesAsync(Shipment shipment, CancellationToken cancellationToken = default);
    }
}
