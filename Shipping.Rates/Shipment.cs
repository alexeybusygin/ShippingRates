using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Shipping.Rates
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
            Rates = new List<Rate>();
            Errors = new List<Error>();
        }

        public int PackageCount
        {
            get { return Packages.Count; }
        }
        public List<Rate> Rates { get; }
        public decimal TotalPackageWeight
        {
            get { return Packages.Sum(x => x.Weight); }
        }
        public List<Error> Errors { get; }
    }
}
