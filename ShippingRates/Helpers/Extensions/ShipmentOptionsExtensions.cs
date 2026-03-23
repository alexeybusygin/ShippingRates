namespace ShippingRates.Helpers.Extensions
{
    internal static class ShipmentOptionsExtensions
    {
        internal static string GetCurrencyCode(this ShipmentOptions options)
        {
            return !string.IsNullOrEmpty(options?.PreferredCurrencyCode)
                ? options.PreferredCurrencyCode
                : ShipmentOptions.DefaultCurrencyCode;
        }
    }
}
