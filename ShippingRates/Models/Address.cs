using System;
using System.Collections.Generic;

namespace ShippingRates
{
    public class Address
    {
        static readonly HashSet<string> UsAndTerritories = new(StringComparer.OrdinalIgnoreCase)
        {
            "AS", "GU", "MP", "PR", "UM", "VI", "US"
        };

        public Address(string? city, string? state, string? postalCode, string? countryCode) : this(null, null, null, city, state, postalCode, countryCode)
        {
        }

        public Address(string? line1, string? line2, string? line3, string? city, string? state, string? postalCode, string? countryCode, bool isResidential = false)
        {
            Line1 = line1;
            Line2 = line2;
            Line3 = line3;
            City = city;
            State = state;
            PostalCode = postalCode;
            CountryCode = countryCode;
            IsResidential = isResidential;
        }

        public string? City { get; set; }
        public string? CountryCode { get; set; }
        public string? CountryName { get; set; }
        public string? Line1 { get; set; }
        public string? Line2 { get; set; }
        public string? Line3 { get; set; }
        public string? PostalCode { get; set; }
        public string? State { get; set; }
        public bool IsResidential { get; set; }

        /// <summary>
        ///     Returns true if the CountryCode matches US or one of the US territories.
        /// </summary>
        /// <returns></returns>
        public bool IsUnitedStatesAddress()
        {
            return !string.IsNullOrWhiteSpace(CountryCode) && UsAndTerritories.Contains(CountryCode.Trim());
        }
    }
}
