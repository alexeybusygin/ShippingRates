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
        public ReadOnlyCollection<Package> Packages;
        public ICollection<IRateAdjuster> RateAdjusters;
        public readonly Address DestinationAddress;
        public readonly Address OriginAddress;

        public Shipment(Address originAddress, Address destinationAddress, List<Package> packages)
        {
            OriginAddress = originAddress;
            DestinationAddress = destinationAddress;
            Packages = packages.AsReadOnly();
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
