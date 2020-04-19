using System;
using System.Linq;
using System.Threading.Tasks;

namespace ShippingRates.ShippingProviders
{
    /// <summary>
    ///     A base implementation of the <see cref="IShippingProvider" /> interface.
    ///     All provider-specific classes should inherit from this class.
    /// </summary>
    public abstract class AbstractShippingProvider : IShippingProvider
    {
        public virtual async Task GetRates()
        {
        }

        public string Name { get; set; }
        public Shipment Shipment { get; set; }

        protected void AddError(Error error)
        {
            lock (Shipment)
            {
                Shipment.Errors.Add(error);
            }
        }

        protected void AddInternalError(string error)
        {
            lock (Shipment)
            {
                Shipment.InternalErrors.Add(error);
            }
        }

        protected void AddRate(string providerCode, string name, decimal totalCharges, DateTime delivery)
        {
            AddRate(new Rate(Name, providerCode, name, totalCharges, delivery));
        }

        protected void AddRate(Rate rate)
        {
            lock (Shipment)
            {
                if (Shipment.RateAdjusters != null)
                {
                    rate = Shipment.RateAdjusters.Aggregate(rate, (current, adjuster) => adjuster.AdjustRate(current));
                }
                Shipment.Rates.Add(rate);
            }
        }
    }
}
