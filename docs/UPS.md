# UPS provider

Use `UPSProvider` to retrieve UPS shipping rates through the UPS OAuth-based Rating API.

## Applies to

- ShippingRates 4.x

## Prerequisites

- UPS OAuth `ClientId`
- UPS OAuth `ClientSecret`
- UPS `AccountNumber`

## Create the provider

```csharp
using System.Net.Http;
using ShippingRates.ShippingProviders;

var configuration = new UPSProviderConfiguration
{
    ClientId = upsClientId,
    ClientSecret = upsClientSecret,
    AccountNumber = upsAccountNumber,
    UseProduction = false
};

using var httpClient = new HttpClient();

var provider = new UPSProvider(configuration, httpClient);
```

## Constructors

### `UPSProvider(UPSProviderConfiguration configuration)`

Creates a provider that manages its own `HttpClient`.

### `UPSProvider(UPSProviderConfiguration configuration, HttpClient httpClient)`

Creates a provider that uses the supplied `HttpClient`.

Use this overload when you want shared connection management, custom timeouts, or dependency-injected HTTP behavior.

### `UPSProvider(UPSProviderConfiguration configuration, ILogger<UPSProvider> logger)`

Creates a provider with logging and an internally managed `HttpClient`.

### `UPSProvider(UPSProviderConfiguration configuration, HttpClient httpClient, ILogger<UPSProvider> logger)`

Creates a provider with both a supplied `HttpClient` and logging.

## Configuration reference

### Required properties

| Property | Type | Description |
| --- | --- | --- |
| `ClientId` | `string` | UPS OAuth client ID. Required. |
| `ClientSecret` | `string` | UPS OAuth client secret. Required. |
| `AccountNumber` | `string` | UPS shipper account number. Required. |

### Optional properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `UseProduction` | `bool` | `false` | Uses the UPS production API when `true`; otherwise uses the test environment. |
| `ServiceDescription` | `string` | `null` | Restricts results to one UPS service description, such as `"UPS Ground"`. If omitted, all supported services are requested. |
| `UseRetailRates` | `bool` | `false` | Requests retail rates. Overrides `CustomerClassification`. |
| `UseDailyRates` | `bool` | `false` | Requests daily rates. Overrides `CustomerClassification`. |
| `UseNegotiatedRates` | `bool` | `false` | Requests negotiated/account-specific rates when the UPS account is approved for them. |
| `CustomerClassification` | `UPSCustomerClassification` | `ShipperNumberRates` | Customer classification code for US-origin shipments. Ignored for non-US shipments. |

## Common scenarios

### Request a specific UPS service

Use `ServiceDescription` when you want one service instead of the full rate set.

```csharp
var configuration = new UPSProviderConfiguration
{
    ClientId = upsClientId,
    ClientSecret = upsClientSecret,
    AccountNumber = upsAccountNumber,
    UseProduction = false,
    ServiceDescription = "UPS Ground"
};
```

Supported values are exposed by `UPSProvider.GetServiceCodes()`.

### Request negotiated rates

```csharp
var configuration = new UPSProviderConfiguration
{
    ClientId = upsClientId,
    ClientSecret = upsClientSecret,
    AccountNumber = upsAccountNumber,
    UseProduction = true,
    UseNegotiatedRates = true
};
```

UPS returns negotiated rates only for accounts that are approved to use them.

### Select the customer classification

`CustomerClassification` matters for US-origin shipments. In many integrations the default value is correct, but you can override it explicitly:

```csharp
var configuration = new UPSProviderConfiguration
{
    ClientId = upsClientId,
    ClientSecret = upsClientSecret,
    AccountNumber = upsAccountNumber,
    CustomerClassification = UPSCustomerClassification.DailyRates
};
```

If `UseRetailRates` or `UseDailyRates` is set, that override takes precedence over `CustomerClassification`.

## Service descriptions

`UPSProvider.GetServiceCodes()` returns the supported UPS service-code-to-description map used by ShippingRates.

Common descriptions include:

- `UPS Ground`
- `UPS Next Day Air`
- `UPS Second Day Air`
- `UPS 3-Day Select`
- `UPS Worldwide Express`

## Limitations and considerations

- `UPSProviderConfiguration` validates `ClientId`, `ClientSecret`, and `AccountNumber` during provider construction.
- `UseNegotiatedRates` returns account-specific pricing only when the UPS account is approved for negotiated rates.
- `ServiceDescription` must match one of the service descriptions known to the library. Unknown values do not create a fuzzy match.
- `CustomerClassification` applies only to US-origin shipments.
- `UseRetailRates` and `UseDailyRates` override `CustomerClassification`.
- Saturday-delivery requests are controlled through `ShipmentOptions.SaturdayDelivery`, not UPS-specific configuration.
