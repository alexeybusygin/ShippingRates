This page documents breaking changes introduced in major and minor versions of ShippingRates.

# Version 4.0.0

## FedEx Integration Reworked (REST + OAuth 2.0)

Starting with v4.0.0, FedEx providers use the modern FedEx REST API with OAuth 2.0 configuration only.

SOAP-era FedEx credentials and constructors were removed.

More details are available in the official FedEx migration guide: https://developer.fedex.com/api/en-us/guides/migrate.html

### What Changed

* `FedExProvider` and `FedExSmartPostProvider` now require `FedExProviderConfiguration`
* Legacy constructor arguments (`key`, `password`, `meterNumber`) are no longer supported
* `FedExProviderConfiguration.Key`, `Password`, and `MeterNumber` were removed
* FedEx provider types now live under `ShippingRates.ShippingProviders.FedEx`

### Required Action

To migrate from v3.x to v4.0.0:

1. Update imports to include `ShippingRates.ShippingProviders.FedEx`
2. Replace old FedEx provider constructors with `FedExProviderConfiguration`
3. Use OAuth credentials (`ClientId`, `ClientSecret`) and `AccountNumber`
4. Set `HubId` in configuration when using `FedExSmartPostProvider`

### Migration Example

Before (v3.x):

```csharp
rateManager.AddProvider(new FedExProvider(fedexKey, fedexPassword, fedexAccountNumber, fedexMeterNumber, fedexUseProduction));
rateManager.AddProvider(new FedExSmartPostProvider(fedexKey, fedexPassword, fedexAccountNumber, fedexMeterNumber, fedexHubId, fedexUseProduction));
```

After (v4.0.0):

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

# Version 3.0.0

## USPS Integration Rewritten (REST + OAuth 2.0)

Starting with v3.0.0, the USPS integration was completely rewritten to use the modern USPS REST APIs with OAuth 2.0 authentication.

Previous versions relied on the legacy USPS Web Tools API, which is not compatible with the new USPS platform.

Upgrading to v3.0.0 or later is required to continue using USPS services.

More details are available at the USPS developer portal: https://developers.usps.com/

### What Changed

* Legacy USPS Web Tools API support removed
* OAuth 2.0 authentication is now required
* `USPSProvider` is now configured exclusively via the `USPSProviderConfiguration` model
* Credential structure and initialization flow have changed

### Required Action

To continue using USPS services:

1. Register at the USPS Developer Portal
2. Obtain OAuth 2.0 credentials
3. Update your provider configuration to use `USPSProviderConfiguration`
4. Adjust initialization code if upgrading from v2.x
