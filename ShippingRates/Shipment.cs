using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ShippingRates
{
    /// <summary>
    ///     Summary description for Shipment.
    /// </summary>
    public class Shipment
    {
        public ReadOnlyCollection<Package> Packages { get; }
        public ICollection<IRateAdjuster> RateAdjusters;
        public Address DestinationAddress { get; }
        public Address OriginAddress { get; }

        public Shipment(Address originAddress, Address destinationAddress, List<Package> packages)
        {
            OriginAddress = originAddress ?? throw new ArgumentNullException(nameof(originAddress));
            DestinationAddress = destinationAddress ?? throw new ArgumentNullException(nameof(destinationAddress));
            Packages = packages?.AsReadOnly() ?? throw new ArgumentNullException(nameof(packages));
        }

        /// <summary>
        ///     Number of packages in the shipment
        /// </summary>
        public int PackageCount =>Packages.Count;
        /// <summary>
        ///     Total shipment weight
        /// </summary>
        public decimal TotalPackageWeight => Packages.Sum(x => x.Weight);
        /// <summary>
        ///     Shipment rates
        /// </summary>
        public List<Rate> Rates { get; } = new List<Rate>();
        /// <summary>
        ///     Errors returned by service provider (e.g. 'Wrong postal code')
        /// </summary>
        public List<Error> Errors { get; } = new List<Error>();
        /// <summary>
        ///     Internal library errors during interaction with service provider
        ///     (e.g. SoapException was trown)
        /// </summary>
        public List<string> InternalErrors { get; } = new List<string>();
    }
}
