using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ShippingRates.RateServiceWebReference;

namespace ShippingRates.Helpers.Extensions
{
    public static class AddressExtensions
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
        private static string[] GetStreetLines(Address address)
        {
            var streetLines = new List<string>
            {
                address.Line1?.Trim(),
                address.Line2?.Trim(),
                address.Line3?.Trim()
            };
            streetLines = streetLines.Where(l => !string.IsNullOrEmpty(l)).ToList();
            return streetLines.Any() ? streetLines.ToArray() : new string[] { "" };
        }
    }
}
