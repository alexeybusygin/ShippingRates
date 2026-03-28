# ShippingRates

[![Build](https://github.com/alexeybusygin/ShippingRates/actions/workflows/build.yml/badge.svg)](https://github.com/alexeybusygin/ShippingRates/actions/workflows/build.yml)
[![NuGet Version](https://img.shields.io/nuget/v/ShippingRates.svg?style=flat-square)](https://www.nuget.org/packages/ShippingRates)

ShippingRates is a .NET library for retrieving shipping rates from UPS, FedEx, USPS, and DHL APIs.

## Why Use It

- Query multiple carriers through one `RateManager`
- Work with sync or async APIs
- Support multiple packages in a single shipment
- Apply shipment-level options such as Saturday delivery
- Extend returned prices with custom rate adjusters

## Supported Platforms

- .NET 6+
- .NET Standard 2.0
- .NET Framework 4.6.1+

## Install

Available on [NuGet](https://www.nuget.org/packages/ShippingRates):

```powershell
dotnet add package ShippingRates
```

## Choose Your Path

- New integration: start with [Quick Start](#quick-start), then open the carrier-specific page you need in [Carrier Setup](#carrier-setup).
- Upgrading from an older version: start with [Breaking changes](docs/Breaking-Changes.md).
- Tuning shared shipment behavior: see [Shipment Options](#shipment-options).
- Looking for deeper docs: open [docs/README.md](docs/README.md).

## Upgrade Notes

ShippingRates `4.x` includes FedEx breaking changes and uses the modern FedEx REST API with OAuth 2.0.

If you are upgrading an existing integration, start with [Breaking changes](docs/Breaking-Changes.md).

## Quick Start

```csharp
using System.Collections.Generic;
using System.Net.Http;
using ShippingRates;
using ShippingRates.ShippingProviders.FedEx;
using ShippingRates.ShippingProviders;
using ShippingRates.ShippingProviders.Usps;

using var httpClient = new HttpClient();

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

The sample app in [SampleApp/Program.cs](SampleApp/Program.cs) shows a fuller end-to-end setup.

## Carrier Setup

Use the carrier-specific setup pages for constructor overloads, configuration options, and examples:

- [UPS provider](docs/UPS.md)
- [FedEx provider](docs/FedEx.md)
- [USPS provider](docs/USPS.md)
- [DHL provider](docs/DHL.md)

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

- [Documentation index](docs/README.md)
- [UPS provider](docs/UPS.md)
- [FedEx provider](docs/FedEx.md)
- [USPS provider](docs/USPS.md)
- [DHL provider](docs/DHL.md)
- [Breaking changes](docs/Breaking-Changes.md)
- [Release notes](docs/Release-Notes.md)
- [HttpClient lifecycle](docs/HttpClient-lifecycle.md)
- [Logging](docs/Logging.md)
- [Rate adjusters](docs/Rate-Adjusters.md)
- [Custom shipping providers](docs/Custom-Shipping-Providers.md)
- [3rd party docs](docs/3rd-Party-Docs.md)

## Credits

Originally forked from [DotNetShipping](https://github.com/kylewest/DotNetShipping) by [@kylewest](https://github.com/kylewest). Package icon by [Fredy Sujono](https://www.iconfinder.com/freud).
