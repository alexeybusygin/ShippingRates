# Breaking Changes

This article lists ShippingRates updates that require code changes.

## Applies to

- ShippingRates 3.x and later

## Version 4.0.0

**Released:** March 2, 2026

### FedEx moved to REST + OAuth 2.0

| Item | Details |
| --- | --- |
| Change type | Breaking |
| Affected area | `FedExProvider`, `FedExSmartPostProvider`, FedEx configuration |
| Impact | Legacy SOAP-style FedEx initialization no longer compiles |
| Required action | Move to `FedExProviderConfiguration` and OAuth credentials |

Starting in `v4.0.0`, FedEx providers use the FedEx REST API and OAuth 2.0 only.

#### What changed

- `FedExProvider` and `FedExSmartPostProvider` require `FedExProviderConfiguration`
- Legacy constructor arguments (`key`, `password`, `meterNumber`) were removed
- `FedExProviderConfiguration.Key`, `Password`, and `MeterNumber` were removed
- FedEx provider types are under `ShippingRates.ShippingProviders.FedEx`

#### Required action

1. Add `using ShippingRates.ShippingProviders.FedEx;`
2. Replace legacy constructors with `FedExProviderConfiguration`
3. Use OAuth credentials (`ClientId`, `ClientSecret`) and `AccountNumber`
4. Set `HubId` when you use `FedExSmartPostProvider`

#### Migration example

Before (`v3.x`):

```csharp
rateManager.AddProvider(new FedExProvider(fedexKey, fedexPassword, fedexAccountNumber, fedexMeterNumber, fedexUseProduction));
rateManager.AddProvider(new FedExSmartPostProvider(fedexKey, fedexPassword, fedexAccountNumber, fedexMeterNumber, fedexHubId, fedexUseProduction));
```

After (`v4.0.0`):

```csharp
using ShippingRates.ShippingProviders.FedEx;

var fedExConfiguration = new FedExProviderConfiguration
{
    ClientId = fedexClientId,
    ClientSecret = fedexClientSecret,
    AccountNumber = fedexAccountNumber,
    HubId = fedexHubId,
    UseProduction = fedexUseProduction
};

rateManager.AddProvider(new FedExProvider(fedExConfiguration));
rateManager.AddProvider(new FedExSmartPostProvider(fedExConfiguration));
```

FedEx migration reference: <https://developer.fedex.com/api/en-us/guides/migrate.html>

## Version 3.0.0

**Released:** December 7, 2025

### USPS moved to REST + OAuth 2.0

| Item | Details |
| --- | --- |
| Change type | Breaking |
| Affected area | USPS provider authentication and initialization |
| Impact | Legacy USPS Web Tools integration is no longer supported |
| Required action | Move to `UspsProviderConfiguration` with OAuth credentials |

Starting in `v3.0.0`, USPS integration uses USPS REST APIs with OAuth 2.0.

#### What changed

- Legacy USPS Web Tools API support was removed
- OAuth 2.0 authentication is required
- `UspsProvider` uses `UspsProviderConfiguration`
- Credential and initialization flow changed

#### Required action

1. Register in the USPS Developer Portal
2. Create OAuth credentials
3. Configure `UspsProviderConfiguration` (`ClientId`, `ClientSecret`, `UseProduction`)
4. Update initialization code if you are upgrading from `v2.x`

USPS developer portal: <https://developers.usps.com/>
