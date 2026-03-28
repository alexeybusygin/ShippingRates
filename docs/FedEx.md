# FedEx provider

Use `FedExProvider` for standard FedEx REST rating and `FedExSmartPostProvider` for SmartPost-only rating.

## Applies to

- ShippingRates 4.x

## Prerequisites

- FedEx OAuth `ClientId`
- FedEx OAuth `ClientSecret`
- FedEx `AccountNumber`
- FedEx `HubId` when you use `FedExSmartPostProvider`

## Create the providers

### FedEx standard rates

```csharp
using System.Net.Http;
using ShippingRates.ShippingProviders.FedEx;

var configuration = new FedExProviderConfiguration
{
    ClientId = fedexClientId,
    ClientSecret = fedexClientSecret,
    AccountNumber = fedexAccountNumber,
    UseProduction = false
};

using var httpClient = new HttpClient();

var provider = new FedExProvider(configuration, httpClient);
```

### FedEx SmartPost

```csharp
var smartPostConfiguration = new FedExProviderConfiguration
{
    ClientId = fedexClientId,
    ClientSecret = fedexClientSecret,
    AccountNumber = fedexAccountNumber,
    HubId = fedexHubId,
    UseProduction = false
};

var smartPostProvider = new FedExSmartPostProvider(smartPostConfiguration, httpClient);
```

## Constructors

### `FedExProvider`

| Constructor | Description |
| --- | --- |
| `FedExProvider(FedExProviderConfiguration configuration)` | Creates a provider with an internally managed `HttpClient`. |
| `FedExProvider(FedExProviderConfiguration configuration, HttpClient httpClient)` | Uses the supplied `HttpClient`. |
| `FedExProvider(FedExProviderConfiguration configuration, ILogger<FedExProvider> logger)` | Adds logging with an internally managed `HttpClient`. |
| `FedExProvider(FedExProviderConfiguration configuration, HttpClient httpClient, ILogger<FedExProvider> logger)` | Uses the supplied `HttpClient` and logger. |

### `FedExSmartPostProvider`

| Constructor | Description |
| --- | --- |
| `FedExSmartPostProvider(FedExProviderConfiguration configuration)` | Creates a SmartPost-only provider with an internally managed `HttpClient`. |
| `FedExSmartPostProvider(FedExProviderConfiguration configuration, HttpClient httpClient)` | Uses the supplied `HttpClient`. |
| `FedExSmartPostProvider(FedExProviderConfiguration configuration, ILogger<FedExSmartPostProvider> logger)` | Adds logging with an internally managed `HttpClient`. |
| `FedExSmartPostProvider(FedExProviderConfiguration configuration, HttpClient httpClient, ILogger<FedExSmartPostProvider> logger)` | Uses the supplied `HttpClient` and logger. |

## Configuration reference

### Required properties

| Property | Type | Description |
| --- | --- | --- |
| `ClientId` | `string` | FedEx OAuth client ID. Required. |
| `ClientSecret` | `string` | FedEx OAuth client secret. Required. |

### Optional properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `AccountNumber` | `string` | `null` | FedEx account number used for account-based rating. |
| `UseProduction` | `bool` | `false` | Uses the FedEx production API when `true`; otherwise uses the sandbox API. |
| `UseNegotiatedRates` | `bool` | `false` | Requests account/negotiated rate types when available. |
| `PickupType` | `FedExPickupType` | `UseScheduledPickup` | Pickup type sent to the FedEx API. |
| `PackagingType` | `FedExPackagingType` | `YourPackaging` | Default packaging type for FedEx requests. |
| `HubId` | `string` | `null` | SmartPost hub ID. Required for SmartPost scenarios. In the FedEx test environment, `5531` can be used per FedEx guidance. |

## Common scenarios

### Request negotiated rates

```csharp
var configuration = new FedExProviderConfiguration
{
    ClientId = fedexClientId,
    ClientSecret = fedexClientSecret,
    AccountNumber = fedexAccountNumber,
    UseProduction = true,
    UseNegotiatedRates = true
};
```

### Set pickup and packaging defaults

```csharp
var configuration = new FedExProviderConfiguration
{
    ClientId = fedexClientId,
    ClientSecret = fedexClientSecret,
    AccountNumber = fedexAccountNumber,
    PickupType = FedExPickupType.DropoffAtFedExLocation,
    PackagingType = FedExPackagingType.FedExPak
};
```

### Enable SmartPost

```csharp
var configuration = new FedExProviderConfiguration
{
    ClientId = fedexClientId,
    ClientSecret = fedexClientSecret,
    AccountNumber = fedexAccountNumber,
    HubId = fedexHubId
};

rateManager.AddProvider(new FedExSmartPostProvider(configuration, httpClient));
```

## Reference values

### `FedExPickupType`

- `ContactFedExToSchedule`
- `DropoffAtFedExLocation`
- `UseScheduledPickup`

### `FedExPackagingType`

- `YourPackaging`
- `FedExEnvelope`
- `FedExPak`
- `FedExBox`
- `FedExTube`
- `FedEx10KgBox`
- `FedEx25KgBox`
- `FedExSmallBox`
- `FedExMediumBox`
- `FedExLargeBox`
- `FedExExtraLargeBox`

## Shipment options that affect FedEx

FedEx also responds to shared `ShipmentOptions` values:

- `PreferredCurrencyCode`
- `FedExOneRate`
- `FedExPackagingTypeOverride`
- `FedExOneRatePackageOverride` (legacy)
- `SaturdayDelivery`
- `ShippingDate`

## Limitations and considerations

- `FedExProvider` excludes SmartPost services. Use `FedExSmartPostProvider` for SmartPost-only rating.
- `FedExSmartPostProvider` requires `HubId` in practical use.
- `FedExSmartPostProvider` does not send insured values.
- `UseNegotiatedRates` changes which FedEx rate types are selected from the response. If negotiated/account rates are unavailable, the result set can differ from list-rate expectations.
- `FedExOneRate`, `PreferredCurrencyCode`, and packaging overrides are controlled through shared `ShipmentOptions`, not `FedExProviderConfiguration`.
- `FedExProviderConfiguration` validates `ClientId` and `ClientSecret` during provider construction.
