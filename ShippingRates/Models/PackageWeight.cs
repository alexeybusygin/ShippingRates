using System;

namespace ShippingRates.Models
{
    internal class PackageWeight
    {
        readonly UnitsSystem _unitsSystem;
        readonly decimal _value;

        public PackageWeight(UnitsSystem unitsSystem, decimal value)
        {
            _unitsSystem = unitsSystem;
            _value = value;
        }

        public decimal Get() => _value;

        public decimal Get(UnitsSystem unitsSystem)
        {
            if (unitsSystem == _unitsSystem)
            {
                return _value;
            }
            else if (unitsSystem == UnitsSystem.Metric && _unitsSystem == UnitsSystem.USCustomary)
            {
                return _value * 0.45359237m;
            }
            else if (unitsSystem == UnitsSystem.USCustomary && _unitsSystem == UnitsSystem.Metric)
            {
                return _value * 2.20462m;
            }
            throw new Exception($"Unsupported weight conversion from {_unitsSystem} to {unitsSystem}");
        }

        public decimal GetRounded(UnitsSystem unitsSystem) => Math.Ceiling(Get(unitsSystem));
    }
}
