namespace ShippingRates
{
    /// <summary>
    ///     Pounds and ounces are used for the USPS provider.
    /// </summary>
    public struct PoundsAndOunces
    {
        public int Pounds { get; set; }
        public decimal Ounces { get; set; }
    }
}
