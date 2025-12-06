using System;
using System.Linq;

namespace ShippingRates.Models.Ups
{
    internal class UpsAddress
    {
        public string[] AddressLine = Array.Empty<string>();
        public string City { get; set; }
        public string StateProvinceCode { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
        public string ResidentialAddressIndicator { get; set; }

        public UpsAddress(Address address)
        {
            var addressLines = new string[] { address.Line1, address.Line2, address.Line3 }
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            AddressLine = addressLines;
            City = address.City;
            StateProvinceCode = address.State;
            PostalCode = address.PostalCode;
            CountryCode = address.CountryCode;
        }
    }
}
