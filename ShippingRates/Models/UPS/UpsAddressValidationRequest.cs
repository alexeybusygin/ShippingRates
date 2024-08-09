using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace ShippingRates.Models.UPS
{
    internal class UpsAddressValidationRequest
    {
        [JsonPropertyName("XAVRequest")]
        public XAVRequest XAVRequest { get; set; }

        public UpsAddressValidationRequest(Address address)
        {
            XAVRequest = new XAVRequest(address);
        }
    }

    class XAVRequest
    {
        public AddressKeyFormat AddressKeyFormat { get; set; }

        public XAVRequest(Address address)
        {
            AddressKeyFormat = new AddressKeyFormat(address);
        }
    }

    class AddressKeyFormat
    {
        public AddressKeyFormat(Address address)
        {
            var addressLines = new string[] { address.Line1, address.Line2, address.Line3 }
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            AddressLine = addressLines;
            Region = $"{address.City},{address.State},{address.PostalCode}";
            CountryCode = address.CountryCode;
        }

        public string[] AddressLine = Array.Empty<string>();
        public string Region { get; set; }
        public string CountryCode { get; set; }
    }
}
