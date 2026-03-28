# USPS provider

Use `UspsProvider` to retrieve USPS domestic and international rates through the USPS REST API with OAuth.

## Applies to

- ShippingRates 3.x and later

## Prerequisites

- USPS OAuth `ClientId`
- USPS OAuth `ClientSecret`

## Create the provider

```csharp
using System.Net.Http;
using ShippingRates.ShippingProviders.Usps;

var configuration = new UspsProviderConfiguration
{
    ClientId = uspsClientId,
    ClientSecret = uspsClientSecret,
    UseProduction = false
};

using var httpClient = new HttpClient();

var provider = new UspsProvider(configuration, httpClient);
```

The same `UspsProvider` handles both domestic and international shipments. There is no separate international provider in current versions.

## Constructors

### `UspsProvider(UspsProviderConfiguration configuration)`

Creates a provider that manages its own `HttpClient`.

### `UspsProvider(UspsProviderConfiguration configuration, HttpClient httpClient)`

Creates a provider that uses the supplied `HttpClient`.

### `UspsProvider(UspsProviderConfiguration configuration, ILogger<UspsProvider> logger)`

Creates a provider with logging and an internally managed `HttpClient`.

### `UspsProvider(UspsProviderConfiguration configuration, HttpClient httpClient, ILogger<UspsProvider> logger)`

Creates a provider with both a supplied `HttpClient` and logging.

## Configuration reference

### Required properties

| Property | Type | Description |
| --- | --- | --- |
| `ClientId` | `string` | USPS OAuth client ID. Required. |
| `ClientSecret` | `string` | USPS OAuth client secret. Required. |

### Optional properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `UseProduction` | `bool` | `false` | Uses the USPS production API when `true`; otherwise uses the test environment. |
| `PriceType` | `UspsPriceType` | `COMMERCIAL` | USPS price table preference. This influences which prices USPS considers. |
| `ProcessingCategory` | `UspsProcessingCategory` | `MACHINABLE` | Filters rates to the selected USPS processing category. |
| `MailClasses` | `UspsMailClass[]` | `[UspsMailClass.All]` | Mail classes to request. `All` asks USPS to return all qualifying classes. |
| `ExtraServiceCodes` | `UspsExtraServiceCode[]` | `[]` | Extra services such as insurance or signature confirmation. |
| `AccountType` | `string` | `null` | USPS account type for scenarios that require a specific commercial contract/account shape. |
| `AccountNumber` | `string` | `null` | USPS account number associated with the selected commercial account. |

## Common scenarios

### Request a specific mail class

Use `MailClasses` to limit the result set instead of accepting every qualifying class.

```csharp
var configuration = new UspsProviderConfiguration
{
    ClientId = uspsClientId,
    ClientSecret = uspsClientSecret,
    MailClasses = [UspsMailClass.PriorityMail]
};
```

### Request online/commercial pricing

`COMMERCIAL` is already the default and should be used in most integrations.

```csharp
var configuration = new UspsProviderConfiguration
{
    ClientId = uspsClientId,
    ClientSecret = uspsClientSecret,
    PriceType = UspsPriceType.COMMERCIAL
};
```

### Request contract pricing

```csharp
var configuration = new UspsProviderConfiguration
{
    ClientId = uspsClientId,
    ClientSecret = uspsClientSecret,
    PriceType = UspsPriceType.CONTRACT,
    AccountType = uspsAccountType,
    AccountNumber = uspsAccountNumber
};
```

### Request extra services

```csharp
var configuration = new UspsProviderConfiguration
{
    ClientId = uspsClientId,
    ClientSecret = uspsClientSecret,
    MailClasses = [UspsMailClass.PriorityMail],
    ExtraServiceCodes =
    [
        UspsExtraServiceCode.RegisteredMail,
        UspsExtraServiceCode.SignatureConfirmation
    ]
};
```

When extra services are included, specify `MailClasses` explicitly whenever possible. USPS may silently omit incompatible services from broad searches.

## Automatic extra services

The provider adds certain USPS extra services automatically based on the shipment:

- If any package has `SignatureRequiredOnDelivery`, the provider adds `AdultSignatureRequired` and `CertifiedMailAdultSignatureRequired`.
- If the total insured value is greater than `0`, the provider adds either `InsuranceUpTo500` or `InsuranceOver500`.

## Reference values

### `UspsPriceType`

- `RETAIL`
- `COMMERCIAL`
- `CONTRACT`

### `UspsProcessingCategory`

- `MACHINABLE`
- `FLATS`
- `NONSTANDARD`

### `UspsMailClass`

Domestic:

- `All`
- `ParcelSelect`
- `PriorityMailExpress`
- `PriorityMail`
- `LibraryMail`
- `MediaMail`
- `BoundPrintedMatter`
- `UspsConnectLocal`
- `UspsConnectMail`
- `UspsConnectRegional`
- `UspsGroundAdvantage`

International:

- `FirstClassPackageInternationalService`
- `PriorityMailInternational`
- `PriorityMailExpressInternational`
- `GlobalExpressGuaranteed`

### `UspsExtraServiceCode`

`UspsExtraServiceCode` contains the supported USPS special-service codes, including:

- Insurance
- Signature confirmation
- Registered mail
- Return receipt
- Hazardous-material service codes
- Sunday delivery

See [UspsExtraServiceCode.cs](../ShippingRates/ShippingProviders/Usps/UspsExtraServiceCode.cs) for the full enum list used by the library.

## Limitations and considerations

- `UspsProvider` supports only US-origin shipments. If the origin is outside the United States, the provider returns an internal error.
- Domestic and international rating both use `UspsProvider`; there is no separate international provider.
- When `ExtraServiceCodes` are used, broad `MailClasses = [UspsMailClass.All]` searches can hide services unexpectedly. Prefer explicit mail classes.
- `PriceType` is a request preference, not a guarantee that USPS will return that exact pricing table for every shipment.
- `ProcessingCategory` filters returned rates, so mismatched values can make valid USPS services appear to be missing.
- `UspsProviderConfiguration` validates `ClientId` and `ClientSecret` during provider construction.
- Saturday-delivery requests are controlled through `ShipmentOptions.SaturdayDelivery`, not USPS-specific configuration.
