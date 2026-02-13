using System;
using System.Collections.Generic;
using System.Linq;

namespace ShippingRates.Models.Ups;

internal sealed class UpsAddress
{
    public IReadOnlyList<string> AddressLine { get; } = [];
    public string? City { get; }
    public string? StateProvinceCode { get; }
    public string? PostalCode { get; }
    public string? CountryCode { get; }
    public string? ResidentialAddressIndicator { get; set; }

    public UpsAddress(Address address)
    {
        address = address ?? throw new ArgumentNullException(nameof(address));

        AddressLine = new[] { address.Line1, address.Line2, address.Line3 }
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!)
            .ToArray();

        City = address.City;
        StateProvinceCode = address.State;
        PostalCode = address.PostalCode;
        CountryCode = address.CountryCode;
    }
}
