using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingRates.Models
{
    internal class PackageDimension
    {
        readonly UnitsSystem _unitsSystem;
        readonly decimal _value;

        public PackageDimension(UnitsSystem unitsSystem, decimal value)
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
                return _value * 2.54m;
            }
            else if (unitsSystem == UnitsSystem.USCustomary && _unitsSystem == UnitsSystem.Metric)
            {
                return _value * 0.393701m;
            }
            throw new Exception($"Unsupported size conversion from {_unitsSystem} to {unitsSystem}");
        }

        public decimal GetRounded(UnitsSystem unitsSystem) => Math.Ceiling(Get(unitsSystem));
    }
}
