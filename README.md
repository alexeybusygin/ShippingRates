# ShippingRates

[![Build](https://github.com/alexeybusygin/ShippingRates/actions/workflows/build.yml/badge.svg)](https://github.com/alexeybusygin/ShippingRates/actions/workflows/build.yml)
[![NuGet Version](https://img.shields.io/nuget/v/ShippingRates.svg?style=flat-square)](https://www.nuget.org/packages/ShippingRates)

.NET wrapper for UPS, FedEx, USPS, and DHL APIs. Use it to retrieve shipping rates from these carriers.

## FedEx Breaking Changes

Version **4.0.0** includes breaking changes and now supports the modern **FedEx REST API** with OAuth 2.0.

As FedEx API migration timelines approach, please review the migration guidance in the [Breaking Changes](docs/Breaking-Changes.md).

## How to Install

Available in the [NuGet Gallery](http://nuget.org/packages/ShippingRates):

```
PM> Install-Package ShippingRates
```

## Getting Started

```CSharp
// Create RateManager
using var httpClient = new HttpClient();
var rateManager = new RateManager();

// Add desired shipping providers
// You will need an OAuth Client ID, Client Secret, and Account Number to use the UPS provider.
var upsConfiguration = new UPSProviderConfiguration()
{
    ClientId = upsClientId,
    ClientSecret = upsClientSecret,
    AccountNumber = upsAccountNumber,
    UseProduction = false
};
rateManager.AddProvider(new UPSProvider(upsConfiguration, httpClient));

// You will need an account # and meter # to utilize the FedEx provider.
rateManager.AddProvider(new FedExProvider(fedexKey, fedexPassword, fedexAccountNumber, fedexMeterNumber));

// You will need an OAuth Client ID and Client Secret to use the USPS provider.
var uspsProviderConfiguration = new UspsProviderConfiguration()
{
    ClientId = upsClientId,
    ClientSecret = upsClientSecret,
    UseProduction = false
};
rateManager.AddProvider(new UspsProvider(uspsProviderConfiguration, httpClient));

// You will need a Site ID and Password to use the DHL provider.
var dhlConfiguration = new DHLProviderConfiguration(dhlSiteId, dhlPassword, useProduction: false));
rateManager.AddProvider(new DHLProvider(dhlConfiguration, httpClient));

// Setup package and destination/origin addresses
var packages = new List<Package>();
packages.Add(new Package(12, 12, 12, 35, 150));    // Package in lbs and inches
packages.Add(new PackageKgCm(4, 4, 6, 15, 250));   // Package in kg and cm

var origin = new Address("", "", "06405", "US");
var destination = new Address("", "", "20852", "US"); // US Address

// Call GetRates()
Shipment shipment = await rateManager.GetRatesAsync(origin, destination, packages);

// Iterate through the rates returned
foreach (Rate rate in shipment.Rates)
{
    Console.WriteLine(rate);
}
```

See the sample app in this repository for a working example.

## [Documentation](docs)

* [Breaking Changes](docs/Breaking-Changes.md)
* [Release Notes](docs/Release-Notes.md)
* [HttpClient lifecycle](docs/HttpClient-lifecycle.md)
* [Negotiated Rates](docs/Negotiated-Rates.md)
* [Logging](docs/Logging.md)
* [USPS: International Rates](docs/USPS-International-Rates.md)
* [USPS: Extra Services](docs/USPS-Extra-Services.md)
* [Single Rate for UPS and USPS](docs/Single-Rate-for-UPS-and-USPS.md)
* [Rate Adjusters](docs/Rate-Adjusters.md)

## Shipping Options

Shipping options can be passed to the `GetRates` function as a `ShipmentOptions` object.

```CSHARP
var shipment = await rateManager.GetRatesAsync(origin, destination, packages,
    new ShipmentOptions() {
        SaturdayDelivery = true,
        ShippingDate = new DateTime(2020, 7, 15),
        PreferredCurrencyCode = "EUR",                  // For FedEx only
        FedExOneRate = true,                            // For FedEx only
        FedExPackagingTypeOverride = FedExPackagingType.FedExEnvelope // For FedEx only
    });
```

The following options are available:

| Name | Default Value | Meaning |
| ---- | ------------- | ------- |
| SaturdayDelivery | False | Enable the Saturday Delivery option for shipping rates. |
| ShippingDate | null | Pickup date. The current date and time are used if not specified. |
| PreferredCurrencyCode | USD | Preferred rates currency code in the ISO format. Applies to FedEx only. |
| FedExOneRate | False | Use the FedEx One Rate pricing option. Applies to FedEx only. |
| FedExPackagingTypeOverride | null | FedEx packaging type override for this shipment. |
| FedExOneRatePackageOverride | null | Legacy string override for FedEx One Rate packaging. Prefer `FedExPackagingTypeOverride`. |

### Saturday Delivery

If `ShipmentOptions.SaturdayDelivery` is set, you can expect to receive some Saturday Delivery methods. You can check it with the `Rate.Options.SaturdayDelivery` property:

```CSHARP
var anySaturdayDeliveryMethods = shipment.Rates.Any(r => r.Options.SaturdayDelivery);
```    

## Error Handling

Normally, `RateManager.GetRates` wouldn't throw any exceptions. All errors are caught and reported in two properties: `Errors` and `InternalErrors`. `Errors` are for errors coming from APIs (incorrect address, etc.) It should be quite safe to show them to the end user. `InternalErrors` are errors that occur during API calls processing (SOAP, HTTP requests) and errors from inside the ShippingRates. They can be used for debugging and internal reporting. Iterating through Errors and InternalErrors:

```CSHARP
var shipment = rateManager.GetRates(origin, destination, packages);

foreach (var error in shipment.Errors)
{
    Console.WriteLine(error.Number);
    Console.WriteLine(error.Source);
    Console.WriteLine(error.Description);
}

foreach (var error in shipment.InternalErrors)
{
    Console.WriteLine(error);
}
```

#### FedEx and 556 There are no valid services available

This one can be tricky to debug. Start by setting at least $1 insurance for your shipment. For some reason, FedEx will not report errors like the wrong ZIP code for the origin address if no insurance is set.

## Credits

Originally forked from [DotNetShipping](https://github.com/kylewest/DotNetShipping) by [@kylewest](https://github.com/kylewest).
Package icon by [Fredy Sujono](https://www.iconfinder.com/freud).
