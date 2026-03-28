# DHL provider

Use `DHLProvider` to retrieve DHL Express rates.

## Applies to

- ShippingRates 4.x

## Prerequisites

- DHL `SiteId`
- DHL `Password`

## Create the provider

```csharp
using System.Net.Http;
using ShippingRates.ShippingProviders;

var configuration = new DHLProviderConfiguration(dhlSiteId, dhlPassword, useProduction: false);

using var httpClient = new HttpClient();

var provider = new DHLProvider(configuration, httpClient);
```

## Constructors

### Preferred constructors

| Constructor | Description |
| --- | --- |
| `DHLProvider(DHLProviderConfiguration configuration)` | Creates a provider with an internally managed `HttpClient`. |
| `DHLProvider(DHLProviderConfiguration configuration, HttpClient httpClient)` | Uses the supplied `HttpClient`. |

### Obsolete constructors

| Constructor | Description |
| --- | --- |
| `DHLProvider(string siteId, string password, bool useProduction)` | Obsolete. Prefer `DHLProviderConfiguration`. |
| `DHLProvider(string siteId, string password, bool useProduction, char[] services)` | Obsolete. Prefer `DHLProviderConfiguration.IncludeServices`. |
| `DHLProvider(string siteId, string password, bool useProduction, char[] services, int timeout)` | Obsolete. Prefer `DHLProviderConfiguration` plus a configured `HttpClient`. |

## Configuration reference

### Constructor parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `siteId` | `string` | DHL site ID. Required. |
| `password` | `string` | DHL password. Required. |
| `useProduction` | `bool` | Uses the production API when `true`; otherwise uses the test environment. |

### Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `SiteId` | `string` | constructor value | DHL site ID. |
| `Password` | `string` | constructor value | DHL password. |
| `UseProduction` | `bool` | constructor value | Environment selection for DHL API requests. |
| `PaymentAccountNumber` | `string` | `null` | Payment account number for account-specific pricing scenarios. |
| `TimeOut` | `int` | `10` | Obsolete. Will be ignored in future versions. Prefer configuring timeout on `HttpClient`. |
| `ServicesIncluded` | `IReadOnlyCollection<char>` | empty | Read-only view of explicitly included DHL service codes. |
| `ServicesExcluded` | `IReadOnlyCollection<char>` | empty | Read-only view of explicitly excluded DHL service codes. |

## Configuration methods

### `IncludeServices(char[] services)`

Adds a whitelist of DHL service codes. Only those services are returned.

### `ExcludeServices(char[] services)`

Adds a blacklist of DHL service codes. Matching services are filtered out.

## Common scenarios

### Request account-specific pricing

```csharp
var configuration = new DHLProviderConfiguration(dhlSiteId, dhlPassword, useProduction: true)
{
    PaymentAccountNumber = paymentAccountNumber
};
```

### Include only selected services

```csharp
var configuration = new DHLProviderConfiguration(dhlSiteId, dhlPassword, useProduction: false)
    .IncludeServices(['D', 'U', 'X']);
```

### Exclude a specific service

```csharp
var configuration = new DHLProviderConfiguration(dhlSiteId, dhlPassword, useProduction: true)
    .ExcludeServices(['C']);
```

## Service codes

ShippingRates exposes the known DHL service map through `DHLProvider.AvailableServices`.

Common service codes include:

- `D` for `EXPRESS WORLDWIDE`
- `U` for `EXPRESS WORLDWIDE`
- `X` for `EXPRESS ENVELOPE`
- `H` for `ECONOMY SELECT`

## Limitations and considerations

- `DHLProvider` uses TLS 1.2 for internally managed HTTP clients.
- `TimeOut` is obsolete. Prefer supplying an `HttpClient` with the timeout you want.
- `IncludeServices` and `ExcludeServices` filter by DHL service code, not by display name.
- If both include and exclude filters are used, excluded services are still removed from the included set.
- `PaymentAccountNumber` is required only for account-specific pricing scenarios, not for basic rating.
- Saturday-delivery requests are controlled through `ShipmentOptions.SaturdayDelivery`, not DHL-specific configuration.
