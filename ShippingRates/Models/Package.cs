using ShippingRates.Models;
using System;

namespace ShippingRates
{
    /// <summary>
    ///     Summary description for Package.
    /// </summary>
    public class Package
    {
        readonly UnitsSystem _unitsSystem;
        PackageWeight _weight;
        PackageDimension _length;
        PackageDimension _width;
        PackageDimension _height;

        protected Package(UnitsSystem unitsSystem, decimal length, decimal width, decimal height, decimal weight)
        {
            _unitsSystem = unitsSystem;
            Weight = weight;
            Length = length;
            Width = width;
            Height = height;
        }

        /// <summary>
        ///     Creates a new package object.
        /// </summary>
        /// <param name="length">The length of the package, in inches.</param>
        /// <param name="width">The width of the package, in inches.</param>
        /// <param name="height">The height of the package, in inches.</param>
        /// <param name="weight">The weight of the package, in pounds.</param>
        /// <param name="insuredValue">The insured-value of the package, in dollars.</param>
        /// <param name="container">A specific packaging from a shipping provider. E.g. "LG FLAT RATE BOX" for USPS</param>
        /// <param name="signatureRequiredOnDelivery">If true, will attempt to send this to the appropriate rate provider.</param>
        public Package(int length, int width, int height, int weight, decimal insuredValue, string container = null, bool signatureRequiredOnDelivery = false)
            : this(length, width, height, (decimal) weight, insuredValue, container, signatureRequiredOnDelivery)
        {
        }

        /// <summary>
        ///     Creates a new package object.
        /// </summary>
        /// <param name="length">The length of the package, in inches.</param>
        /// <param name="width">The width of the package, in inches.</param>
        /// <param name="height">The height of the package, in inches.</param>
        /// <param name="weight">The weight of the package, in pounds.</param>
        /// <param name="insuredValue">The insured-value of the package, in dollars.</param>
        /// <param name="container">A specific packaging from a shipping provider. E.g. "LG FLAT RATE BOX" for USPS</param>
        /// <param name="signatureRequiredOnDelivery">If true, will attempt to send this to the appropriate rate provider.</param>
        public Package(decimal length, decimal width, decimal height, decimal weight, decimal insuredValue, string container = null, bool signatureRequiredOnDelivery = false)
            : this(UnitsSystem.USCustomary, length, width, height, weight)

        {
            InsuredValue = insuredValue;
            Container = container;
            SignatureRequiredOnDelivery = signatureRequiredOnDelivery;
        }

        public decimal GetCalculatedGirth(UnitsSystem unitsSystem)
        {
            var result = (_width.Get(unitsSystem) * 2) + (_height.Get(unitsSystem) * 2);
            return Math.Ceiling(result);
        }

        public decimal Height { get => _height.Get(); set => _height = new PackageDimension(_unitsSystem, value); }
        public decimal Length { get => _length.Get(); set => _length = new PackageDimension(_unitsSystem, value); }
        public decimal Width { get => _width.Get(); set => _width = new PackageDimension(_unitsSystem, value); }
        public decimal Weight { get => _weight.Get(); set => _weight = new PackageWeight(_unitsSystem, value); }

        public decimal InsuredValue { get; set; }
        public bool IsOversize { get; set; }
        public string Container { get; set; }
        public bool SignatureRequiredOnDelivery { get; set; }

        public decimal GetHeight(UnitsSystem unitsSystem) => _height.Get(unitsSystem);
        public decimal GetLength(UnitsSystem unitsSystem) => _length.Get(unitsSystem);
        public decimal GetWidth(UnitsSystem unitsSystem) => _width.Get(unitsSystem);
        public decimal GetWeight(UnitsSystem unitsSystem) => _weight.Get(unitsSystem);

        public decimal GetRoundedHeight(UnitsSystem unitsSystem) => _height.GetRounded(unitsSystem);
        public decimal GetRoundedLength(UnitsSystem unitsSystem) => _length.GetRounded(unitsSystem);
        public decimal GetRoundedWidth(UnitsSystem unitsSystem) => _width.GetRounded(unitsSystem);
        public decimal GetRoundedWeight(UnitsSystem unitsSystem) => _weight.GetRounded(unitsSystem);

        public PoundsAndOunces PoundsAndOunces
        {
            get
            {
                var poundsAndOunces = new PoundsAndOunces();
                var weight = _weight.Get(UnitsSystem.USCustomary);
                if (weight > 0)
                {
                    poundsAndOunces.Pounds = (int)Math.Truncate(weight);
                    poundsAndOunces.Ounces = (weight - poundsAndOunces.Pounds) * 16;
                }

                return poundsAndOunces;
            }
        }
    }
}
