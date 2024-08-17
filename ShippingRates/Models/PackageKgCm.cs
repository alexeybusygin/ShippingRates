namespace ShippingRates
{
    /// <summary>
    ///     Package object with dimensions in kgs and cm
    /// </summary>
    public class PackageKgCm : Package
    {
        /// <summary>
        ///     Creates a new package object with dimensions in kgs and cm
        /// </summary>
        /// <param name="length">The length of the package, in cm.</param>
        /// <param name="width">The width of the package, in cm.</param>
        /// <param name="height">The height of the package, in cm.</param>
        /// <param name="weight">The weight of the package, in kgs.</param>
        /// <param name="insuredValue">The insured-value of the package, in dollars.</param>
        /// <param name="container">A specific packaging from a shipping provider. E.g. "LG FLAT RATE BOX" for USPS</param>
        /// <param name="signatureRequiredOnDelivery">If true, will attempt to send this to the appropriate rate provider.</param>
        public PackageKgCm(int length, int width, int height, int weight, decimal insuredValue, string container = null, bool signatureRequiredOnDelivery = false)
            : this(length, width, height, (decimal)weight, insuredValue, container, signatureRequiredOnDelivery)
        {
        }

        /// <summary>
        ///     Creates a new package object with dimensions in kgs and cm
        /// </summary>
        /// <param name="length">The length of the package, in cm.</param>
        /// <param name="width">The width of the package, in cm.</param>
        /// <param name="height">The height of the package, in cm.</param>
        /// <param name="weight">The weight of the package, in kgs.</param>
        /// <param name="insuredValue">The insured-value of the package, in dollars.</param>
        /// <param name="container">A specific packaging from a shipping provider. E.g. "LG FLAT RATE BOX" for USPS</param>
        /// <param name="signatureRequiredOnDelivery">If true, will attempt to send this to the appropriate rate provider.</param>
        public PackageKgCm(decimal length, decimal width, decimal height, decimal weight, decimal insuredValue, string container = null, bool signatureRequiredOnDelivery = false)
            : base(UnitsSystem.Metric, length, width, height, weight)

        {
            InsuredValue = insuredValue;
            Container = container;
            SignatureRequiredOnDelivery = signatureRequiredOnDelivery;
        }
    }
}
