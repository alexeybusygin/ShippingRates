using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using ShippingRates.ShippingProviders;

namespace ShippingRates
{
    /// <summary>
    ///     Responsible for coordinating the retrieval of rates from the specified providers for a specified shipment.
    /// </summary>
    public class RateManager
    {
        private readonly IList<IRateAdjuster> _adjusters;
        private readonly ArrayList _providers;

        /// <summary>
        ///     Creates a new RateManager instance.
        /// </summary>
        public RateManager()
        {
            _providers = new ArrayList();
            _adjusters = new List<IRateAdjuster>();
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

        private async Task<Shipment> GetRates(Shipment shipment)
        {
            // create an ArrayList of threads, pre-sized to the number of providers.
            var threads = new List<Task>();

            // iterate through the providers.
            foreach (AbstractShippingProvider provider in _providers)
            {
                // assign the shipment to the provider.
                provider.Shipment = shipment;
                // 
                threads.Add(provider.GetRates());
            }

            await Task.WhenAll(threads).ConfigureAwait(false);

            // return our Shipment instance.
            return shipment;
        }

        /// <summary>
        ///     Retrieves rates for all of the specified providers using the specified address and package information.
        /// </summary>
        /// <param name="originAddress">An instance of <see cref="Address" /> specifying the origin of the shipment.</param>
        /// <param name="destinationAddress">An instance of <see cref="Address" /> specifying the destination of the shipment.</param>
        /// <param name="package">An instance of <see cref="Package" /> specifying the package to be rated.</param>
        /// <param name="options">An optional instance of <see cref="ShipmentOptions" /> specifying the shipment options.</param>
        /// <returns>A <see cref="Shipment" /> instance containing all returned rates.</returns>
        public Shipment GetRates(Address originAddress, Address destinationAddress, Package package, ShipmentOptions options = null)
        {
            return GetRates(originAddress, destinationAddress, new List<Package> { package }, options);
        }

        /// <summary>
        ///     Retrieves rates for all of the specified providers using the specified address and package information.
        /// </summary>
        /// <param name="originAddress">An instance of <see cref="Address" /> specifying the origin of the shipment.</param>
        /// <param name="destinationAddress">An instance of <see cref="Address" /> specifying the destination of the shipment.</param>
        /// <param name="package">An instance of <see cref="Package" /> specifying the package to be rated.</param>
        /// <param name="options">An optional instance of <see cref="ShipmentOptions" /> specifying the shipment options.</param>
        /// <returns>A <see cref="Shipment" /> instance containing all returned rates.</returns>
        public async Task<Shipment> GetRatesAsync(Address originAddress, Address destinationAddress, Package package, ShipmentOptions options = null)
        {
            return await GetRatesAsync(originAddress, destinationAddress, new List<Package> { package }, options).ConfigureAwait(false);
        }

        /// <summary>
        ///     Retrieves rates for all of the specified providers using the specified address and packages information.
        /// </summary>
        /// <param name="originAddress">An instance of <see cref="Address" /> specifying the origin of the shipment.</param>
        /// <param name="destinationAddress">An instance of <see cref="Address" /> specifying the destination of the shipment.</param>
        /// <param name="packages">An instance of <see cref="PackageCollection" /> specifying the packages to be rated.</param>
        /// <param name="options">An optional instance of <see cref="ShipmentOptions" /> specifying the shipment options.</param>
        /// <returns>A <see cref="Shipment" /> instance containing all returned rates.</returns>
        public Shipment GetRates(Address originAddress, Address destinationAddress, List<Package> packages, ShipmentOptions options = null)
        {
            return GetRatesAsync(originAddress, destinationAddress, packages, options).Result;
        }

        /// <summary>
        ///     Retrieves rates for all of the specified providers using the specified address and packages information.
        /// </summary>
        /// <param name="originAddress">An instance of <see cref="Address" /> specifying the origin of the shipment.</param>
        /// <param name="destinationAddress">An instance of <see cref="Address" /> specifying the destination of the shipment.</param>
        /// <param name="packages">An instance of <see cref="PackageCollection" /> specifying the packages to be rated.</param>
        /// <param name="options">An optional instance of <see cref="ShipmentOptions" /> specifying the shipment options.</param>
        /// <returns>A <see cref="Shipment" /> instance containing all returned rates.</returns>
        public async Task<Shipment> GetRatesAsync(Address originAddress, Address destinationAddress, List<Package> packages, ShipmentOptions options = null)
        {
            var shipment = new Shipment(originAddress, destinationAddress, packages, options) { RateAdjusters = _adjusters };
            return await GetRates(shipment).ConfigureAwait(false);
        }
    }
}
