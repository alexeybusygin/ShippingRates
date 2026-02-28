This page documents breaking changes introduced in major and minor versions of ShippingRates.

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
