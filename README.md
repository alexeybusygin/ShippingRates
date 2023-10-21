# ShippingRates

[![Build status](https://ci.appveyor.com/api/projects/status/gqq8i6nw932bn01v?svg=true)](https://ci.appveyor.com/project/alexeybusygin/shippingrates/)
[![NuGet Version](https://img.shields.io/nuget/v/ShippingRates.svg?style=flat-square)](https://www.nuget.org/packages/ShippingRates)

.NET wrapper to UPS, FedEx, USPS, and DHL APIs. Use it to retrieve shipping rates from these carriers.

## How to Install

Available in the [NuGet Gallery](http://nuget.org/packages/ShippingRates):

```
PM> Install-Package ShippingRates
```

## Getting Started

```CSharp
// Create RateManager
var rateManager = new RateManager();

// Add desired shipping providers
// You will need a license #, userid, and password to utilize the UPS provider.
rateManager.AddProvider(new UPSProvider(upsLicenseNumber, upsUserId, upsPassword));
// You will need an account # and meter # to utilize the FedEx provider.
rateManager.AddProvider(new FedExProvider(fedexKey, fedexPassword, fedexAccountNumber, fedexMeterNumber));
// You will need a userId to use the USPS provider. Your account will also need access to the production servers.
rateManager.AddProvider(new USPSProvider(uspsUserId));
// You will need a Site ID and Password to use the DHL provider.
rateManager.AddProvider(new DHLProvider(dhlSiteId, dhlPassword, useProduction: false));

// (Optional) Add RateAdjusters
rateManager.AddRateAdjuster(new PercentageRateAdjuster(.9M));

// Setup package and destination/origin addresses
var packages = new List<Package>();
packages.Add(new Package(12, 12, 12, 35, 150));
packages.Add(new Package(4, 4, 6, 15, 250));

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

ShippingRates supports requesting a single rate from UPS and USPS.
To do so, include the rate description as a parameter of the provider constructor.
```CSHARP
rateManager.AddProvider(new USPSProvider(uspsUserId, "Priority Mail"));
rateManager.AddProvider(new UPSProvider(upsLicenseNumber, upsUserId, upsPassword, "UPS Ground"));
````
A list of valid shipping methods can be found in the documentation links below.

See the sample app in this repository for a working example.

More information can be found in [Wiki](https://github.com/alexeybusygin/ShippingRates/wiki).

#### International Rates
USPS requires a separate API call for retrieving rates for international services.

The call works the same but use the USPSInternationalProvider instead. Your current USPS credentials will work with this and will return the available services between the origin and destination addresses.

## Shipping Options

Shipping options can be passed to the `GetRates` function as a `ShipmentOptions` object.

```CSHARP
var shipment = await rateManager.GetRatesAsync(origin, destination, packages,
    new ShipmentOptions() {
        SaturdayDelivery = true,
        ShippingDate = new DateTime(2020, 7, 15),
        PreferredCurrencyCode = "EUR",                  // For FedEx only
        FedExOneRate = true,                            // For FedEx only
        FedExOneRatePackageOverride = "FEDEX_ENVELOPE"  // For FedEx only
    });
```

The following options are available:

| Name | Default Value | Meaning |
| ---- | ------------- | ------- |
| SaturdayDelivery | False | Enable the Saturday Delivery option for shipping rates. |
| ShippingDate | null | Pickup date. The current date and time are used if not specified. |
| PreferredCurrencyCode | USD | Preferred rates currency code in the ISO format. Applies to FedEx only. |
| FedExOneRate | False | Use the FedEx One Rate pricing option. Applies to FedEx only. |
| FedExOneRatePackageOverride | FEDEX_MEDIUM_BOX | Packing option when using FedEx OneRate. |

### Saturday Delivery

If `ShipmentOptions.SaturdayDelivery` is set, then you can expect to receive some Saturday Delivery methods. You can check it with the `Rate.Options.SaturdayDelivery` property:

```CSHARP
var anySaturdayDeliveryMethods = shipment.Rates.Any(r => r.Options.SaturdayDelivery);
```    

## Error Handling

Normally `RateManager.GetRates` wouldn't throw any exceptions. All errors are caught and reported in two properties: `Errors` and `InternalErrors`. `Errors` are for errors coming from APIs (incorrect address etc.) It should be quite safe to show them to the end user. `InternalErrors` are errors that occur during API calls processing (SOAP, HTTP requests) and errors from inside the ShippingRates. They can be used for debugging and internal reporting. Iterating through Errors and InternalErrors:

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

This one can be tricky to debug. Start with setting at least $1 insurance for your shipment. For some reason, FedEx would not report errors like the wrong ZIP code for the origin address if there is no insurance set.

## 3rd Party Docs

Developer documentation is often hard to find. The links below are provided as reference.

* [FedEx](http://www.fedex.com/us/developer/)
* [USPS](https://www.usps.com/business/web-tools-apis/welcome.htm)
* [UPS](https://www.ups.com/upsdeveloperkit)
* [DHL](https://xmlportal.dhl.com/capability_and_qoute#cap_quote)

## Credits

Originally forked from [DotNetShipping](https://github.com/kylewest/DotNetShipping) by [@kylewest](https://github.com/kylewest).
Package icon by [Fredy Sujono](https://www.iconfinder.com/freud).
