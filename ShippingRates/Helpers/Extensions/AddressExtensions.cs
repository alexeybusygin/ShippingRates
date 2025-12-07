using System;
using System.Linq;

namespace ShippingRates.Helpers.Extensions
{
    internal static class AddressExtensions
    {
        /// <summary>
        /// Get FedEx API address from ShippingRates.Address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static RateServiceWebReference.Address GetFedExAddress(this Address address)
        {
            address = address ?? throw new ArgumentNullException(nameof(address));

            return new RateServiceWebReference.Address
            {
                StreetLines = GetStreetLines(address),
                City = address.City?.Trim(),
                StateOrProvinceCode = address.State?.Trim(),
                PostalCode = address.PostalCode?.Trim(),
                CountryCode = address.CountryCode?.Trim(),
                Residential = address.IsResidential,
                ResidentialSpecified = address.IsResidential,
            };
        }

        /// <summary>
        /// Get street lines array
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private static string?[] GetStreetLines(Address address)
        {
            var streetLines = new[]
            {
                address.Line1,
                address.Line2,
                address.Line3
            }
            .Select(l => l?.Trim())
            .Where(l => !string.IsNullOrEmpty(l))
            .ToArray();

            return streetLines.Length > 0
                ? streetLines
                : [string.Empty];
        }
    }
}
