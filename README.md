# ShippingRates

[![Build](https://github.com/alexeybusygin/ShippingRates/actions/workflows/build.yml/badge.svg)](https://github.com/alexeybusygin/ShippingRates/actions/workflows/build.yml)
[![NuGet Version](https://img.shields.io/nuget/v/ShippingRates.svg?style=flat-square)](https://www.nuget.org/packages/ShippingRates)

ShippingRates is a .NET library for retrieving shipping rates from UPS, FedEx, USPS, and DHL APIs.

- Query multiple carriers through one `RateManager`
- Sync and async APIs
- Multiple packages in a single shipment
- Shipment-level options such as Saturday delivery

It is accompanied by the **[shippingrates-mcp](https://github.com/alexeybusygin/shippingrates-mcp)** server for AI assistants.

## Supported Platforms

- .NET 8+
- .NET Standard 2.0
- .NET Framework 4.6.1+

## Install

Available on [NuGet](https://www.nuget.org/packages/ShippingRates):

```powershell
dotnet add package ShippingRates
```

## Upgrade Notes

ShippingRates `4.x` includes FedEx breaking changes and uses the modern FedEx REST API with OAuth 2.0.

If you are upgrading an existing integration, start with [Breaking changes](https://github.com/alexeybusygin/ShippingRates/blob/master/docs/Breaking-Changes.md).

## Quick Start

```csharp
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using ShippingRates;
using ShippingRates.ShippingProviders.FedEx;
using ShippingRates.ShippingProviders;
using ShippingRates.ShippingProviders.Usps;

using var httpClient = new HttpClient(new HttpClientHandler
{
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
});

var rateManager = new RateManager();

var upsConfiguration = new UPSProviderConfiguration
{
    ClientId = upsClientId,
    ClientSecret = upsClientSecret,
    AccountNumber = upsAccountNumber,
    UseProduction = false
};
rateManager.AddProvider(new UPSProvider(upsConfiguration, httpClient));

var fedExConfiguration = new FedExProviderConfiguration
{
    ClientId = fedexClientId,
    ClientSecret = fedexClientSecret,
    AccountNumber = fedexAccountNumber,
    HubId = fedexHubId,
    UseProduction = false
};
rateManager.AddProvider(new FedExProvider(fedExConfiguration, httpClient));

var uspsConfiguration = new UspsProviderConfiguration
{
    ClientId = uspsClientId,
    ClientSecret = uspsClientSecret,
    UseProduction = false
};
rateManager.AddProvider(new UspsProvider(uspsConfiguration, httpClient));

var dhlConfiguration = new DHLProviderConfiguration(dhlSiteId, dhlPassword, useProduction: false);
rateManager.AddProvider(new DHLProvider(dhlConfiguration, httpClient));

var packages = new List<Package>
{
    new Package(12, 12, 12, 35, 150),
    new PackageKgCm(4, 4, 6, 15, 250)
};

var origin = new Address("", "CT", "06405", "US");
var destination = new Address("", "", "20852", "US");

Shipment shipment = await rateManager.GetRatesAsync(origin, destination, packages);

foreach (Rate rate in shipment.Rates)
{
    Console.WriteLine(rate);
}
```

The sample app in [SampleApp/Program.cs](https://github.com/alexeybusygin/ShippingRates/blob/master/SampleApp/Program.cs) shows a fuller end-to-end setup.

## Carrier Setup

Use the carrier-specific setup pages for constructor overloads, configuration options, and examples:

- [UPS provider](https://github.com/alexeybusygin/ShippingRates/blob/master/docs/UPS.md)
- [FedEx provider](https://github.com/alexeybusygin/ShippingRates/blob/master/docs/FedEx.md)
- [USPS provider](https://github.com/alexeybusygin/ShippingRates/blob/master/docs/USPS.md)
- [DHL provider](https://github.com/alexeybusygin/ShippingRates/blob/master/docs/DHL.md)

## Shipment Options

Pass shipment-level options through `ShipmentOptions`:

```csharp
var shipment = await rateManager.GetRatesAsync(
    origin,
    destination,
    packages,
    new ShipmentOptions
    {
        SaturdayDelivery = true,
        ShippingDate = new DateTime(2020, 7, 15),
        PreferredCurrencyCode = "EUR",
        FedExOneRate = true,
        FedExPackagingTypeOverride = FedExPackagingType.FedExEnvelope
    });
```

| Name | Default | Meaning |
| --- | --- | --- |
| `SaturdayDelivery` | `false` | Request Saturday delivery rates when available. |
| `ShippingDate` | `null` | Pickup date. Uses the current date and time when omitted. |
| `PreferredCurrencyCode` | `USD` | Preferred ISO currency code for FedEx rates. |
| `FedExOneRate` | `false` | Enable FedEx One Rate pricing. |
| `FedExPackagingTypeOverride` | `null` | Override the FedEx packaging type for the shipment. |
| `FedExOneRatePackageOverride` | `null` | Legacy FedEx One Rate override. Prefer `FedExPackagingTypeOverride`. |

If `ShipmentOptions.SaturdayDelivery` is enabled, inspect `Rate.Options.SaturdayDelivery` on returned rates:

```csharp
var anySaturdayDeliveryMethods = shipment.Rates.Any(r => r.Options.SaturdayDelivery);
```

## Error Handling

`RateManager.GetRates` and `RateManager.GetRatesAsync` aggregate provider responses into:

- `shipment.Errors` for carrier/API errors that are generally safe to surface to end users
- `shipment.InternalErrors` for internal processing failures and diagnostics

```csharp
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

## Documentation

- [Documentation index](https://github.com/alexeybusygin/ShippingRates/blob/master/docs/README.md)
- [UPS provider](https://github.com/alexeybusygin/ShippingRates/blob/master/docs/UPS.md)
- [FedEx provider](https://github.com/alexeybusygin/ShippingRates/blob/master/docs/FedEx.md)
- [USPS provider](https://github.com/alexeybusygin/ShippingRates/blob/master/docs/USPS.md)
- [DHL provider](https://github.com/alexeybusygin/ShippingRates/blob/master/docs/DHL.md)
- [Breaking changes](https://github.com/alexeybusygin/ShippingRates/blob/master/docs/Breaking-Changes.md)
- [Release notes](https://github.com/alexeybusygin/ShippingRates/blob/master/docs/Release-Notes.md)
- [HttpClient lifecycle](https://github.com/alexeybusygin/ShippingRates/blob/master/docs/HttpClient-lifecycle.md)
- [Logging](https://github.com/alexeybusygin/ShippingRates/blob/master/docs/Logging.md)
- [Rate adjusters](https://github.com/alexeybusygin/ShippingRates/blob/master/docs/Rate-Adjusters.md)
- [Custom shipping providers](https://github.com/alexeybusygin/ShippingRates/blob/master/docs/Custom-Shipping-Providers.md)
- [3rd party docs](https://github.com/alexeybusygin/ShippingRates/blob/master/docs/3rd-Party-Docs.md)

## Credits

Originally forked from [DotNetShipping](https://github.com/kylewest/DotNetShipping) by [@kylewest](https://github.com/kylewest). Package icon by [Fredy Sujono](https://www.iconfinder.com/freud).
