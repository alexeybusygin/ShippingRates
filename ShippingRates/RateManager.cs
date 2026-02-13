using ShippingRates.ShippingProviders;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShippingRates
{
    /// <summary>
    ///     Responsible for coordinating the retrieval of rates from the specified providers for a specified shipment.
    /// </summary>
    public class RateManager
    {
        private readonly List<IRateAdjuster> _adjusters;
        private readonly List<IShippingProvider> _providers;

        /// <summary>
        ///     Creates a new RateManager instance.
        /// </summary>
        public RateManager()
        {
            _providers = [];
            _adjusters = [];
        }

        /// <summary>
        ///     Adds the specified provider to be rated when <see cref="GetRates" /> is called.
        /// </summary>
        /// <param name="provider">A provider-specific implementation of <see cref="ShippingProviders.IShippingProvider" />.</param>
        public void AddProvider(IShippingProvider provider)
        {
            _providers.Add(provider);
        }

        public void AddRateAdjuster(IRateAdjuster adjuster)
        {
            _adjusters.Add(adjuster);
        }

        /// <summary>
        ///     Retrieves rates for all of the specified providers using the specified address and package information.
        /// </summary>
        /// <param name="originAddress">An instance of <see cref="Address" /> specifying the origin of the shipment.</param>
        /// <param name="destinationAddress">An instance of <see cref="Address" /> specifying the destination of the shipment.</param>
        /// <param name="package">An instance of <see cref="Package" /> specifying the package to be rated.</param>
        /// <param name="options">An optional instance of <see cref="ShipmentOptions" /> specifying the shipment options.</param>
        /// <returns>A <see cref="Shipment" /> instance containing all returned rates.</returns>
        public Shipment GetRates(Address originAddress, Address destinationAddress, Package package, ShipmentOptions? options = null)
        {
            return GetRates(originAddress, destinationAddress, [package], options);
        }

        /// <summary>
        ///     Retrieves rates for all of the specified providers using the specified address and package information.
        /// </summary>
        /// <param name="originAddress">An instance of <see cref="Address" /> specifying the origin of the shipment.</param>
        /// <param name="destinationAddress">An instance of <see cref="Address" /> specifying the destination of the shipment.</param>
        /// <param name="package">An instance of <see cref="Package" /> specifying the package to be rated.</param>
        /// <param name="options">An optional instance of <see cref="ShipmentOptions" /> specifying the shipment options.</param>
        /// <returns>A <see cref="Shipment" /> instance containing all returned rates.</returns>
        public async Task<Shipment> GetRatesAsync(Address originAddress, Address destinationAddress, Package package, ShipmentOptions? options = null)
        {
            return await GetRatesAsync(originAddress, destinationAddress, [package], options).ConfigureAwait(false);
        }

        /// <summary>
        ///     Retrieves rates for all of the specified providers using the specified address and packages information.
        /// </summary>
        /// <param name="originAddress">An instance of <see cref="Address" /> specifying the origin of the shipment.</param>
        /// <param name="destinationAddress">An instance of <see cref="Address" /> specifying the destination of the shipment.</param>
        /// <param name="packages">An instance of <see cref="PackageCollection" /> specifying the packages to be rated.</param>
        /// <param name="options">An optional instance of <see cref="ShipmentOptions" /> specifying the shipment options.</param>
        /// <returns>A <see cref="Shipment" /> instance containing all returned rates.</returns>
        public Shipment GetRates(Address originAddress, Address destinationAddress, List<Package> packages, ShipmentOptions? options = null)
        {
            return GetRatesAsync(originAddress, destinationAddress, packages, options).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Retrieves rates for all of the specified providers using the specified address and packages information.
        /// </summary>
        /// <param name="originAddress">An instance of <see cref="Address" /> specifying the origin of the shipment.</param>
        /// <param name="destinationAddress">An instance of <see cref="Address" /> specifying the destination of the shipment.</param>
        /// <param name="packages">An instance of <see cref="PackageCollection" /> specifying the packages to be rated.</param>
        /// <param name="options">An optional instance of <see cref="ShipmentOptions" /> specifying the shipment options.</param>
        /// <returns>A <see cref="Shipment" /> instance containing all returned rates.</returns>
        public async Task<Shipment> GetRatesAsync(Address originAddress, Address destinationAddress, List<Package> packages, ShipmentOptions? options = null)
        {
            var shipment = new Shipment(originAddress, destinationAddress, packages, options);

            // Create a list of tasks that return RateResult
            var tasks = _providers.Select(provider => provider.GetRatesAsync(shipment)).ToList();

            // Await all tasks and collect results
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            // Aggregate results
            var aggregatedRates = new List<Rate>();
            var aggregatedErrors = new List<Error>();
            var aggregatedInternalErrors = new List<string>();

            foreach (var result in results)
            {
                if (result?.Rates != null)
                {
                    var rates = result.Rates;

                    if (_adjusters?.Count > 0)
                    {
                        rates = rates.Select(rate => _adjusters.Aggregate(rate, (current, adjuster) => adjuster.AdjustRate(current))).ToList();
                    }

                    aggregatedRates.AddRange(rates);
                }

                aggregatedErrors.AddRange(result?.Errors ?? []);
                aggregatedInternalErrors.AddRange(result?.InternalErrors ?? []);
            }

            // Aggregate everything into the shipment object
            shipment.Rates.AddRange(aggregatedRates);
            shipment.Errors.AddRange(aggregatedErrors);
            shipment.InternalErrors.AddRange(aggregatedInternalErrors);

            return shipment;
        }
    }
}
