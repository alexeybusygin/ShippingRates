using System;
using System.Collections.Generic;

namespace ShippingRates.Models
{
    internal class RateResultAggregator
    {
        private readonly string _providerName;
        private readonly List<Rate> _rates = new List<Rate>();
        private readonly List<Error> _errors = new List<Error>();
        private readonly List<string> _internalErrors = new List<string>();

        internal RateResultAggregator(string providerName)
        {
            _providerName = providerName;
        }

        internal void AddProviderError(Error error)
        {
            _errors.Add(error);
        }

        internal void AddInternalError(string error)
        {
            _internalErrors.Add(error);
        }

        internal void AddRate(string providerCode, string name, decimal totalCharges, DateTime delivery, RateOptions options, string currencyCode)
        {
            _rates.Add(new Rate(_providerName, providerCode, name, totalCharges, delivery, options, currencyCode));
        }

        internal RateResult Build()
        {
            var result = new RateResult();
            result.Rates.AddRange(_rates);
            result.Errors.AddRange(_errors);
            result.InternalErrors.AddRange(_internalErrors);
            return result;
        }
    }
}
