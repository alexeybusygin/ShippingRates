# Release notes

This article summarizes notable updates by version.

## Applies to

- ShippingRates 3.x and later

## 4.1.1 (July 20, 2026)

### Added

- Added FedEx International Connect Plus rate support by [@S-Andor](https://github.com/S-Andor) in [#115](https://github.com/alexeybusygin/ShippingRates/pull/115)

### Changed

- Improved FedEx residential rate accuracy by including residential address indicators in rate requests by [@dosisod](https://github.com/dosisod) in [#112](https://github.com/alexeybusygin/ShippingRates/pull/112)
- Prevented deprecated FedEx services from being returned in rate results
- Improved USPS rate reliability when API responses contain undocumented price type or destination facility values by [@jason-jepson-govx](https://github.com/jason-jepson-govx) in [#116](https://github.com/alexeybusygin/ShippingRates/pull/116)

### Breaking changes

- None in this release

## 4.1.0 (May 23, 2026)

### Added

- Added FedEx transit time support for eligible rate requests

### Changed

- Updated the primary package target from .NET 6 to .NET 8
- Improved FedEx error handling for compressed API responses

### Breaking changes

- The primary package target changed from `net6.0` to `net8.0`; `netstandard2.0` support remains available

## 4.0.6 (April 5, 2026)

### Changed

- Fixed USPS rate selection when shipping options return multiple rate entries

### Breaking changes

- None in this release

## 4.0.4 (March 22, 2026)

### Changed

- Improved FedEx error handling

### Breaking changes

- None in this release

## 4.0.3 (March 7, 2026)

### Added

- FedEx package type and pickup type support
- Cancellation token support in the rate request flow

### Changed

- Updated HTTP client lifecycle usage to a rent/lease model
- Removed legacy address helper methods
- Internal refactoring for US address detection

### Breaking changes

- None in this release

## 4.0.2 (March 4, 2026)

### Changed

- Fixed FedEx client behavior when `HttpClient` was null on an internal code path

### Breaking changes

- None in this release

## 4.0.0 (March 2, 2026)

### Added

- FedEx REST API integration for modern Rate and Transit Time workflows
- OAuth 2.0 authentication for FedEx provider flows

### Changed

- Documentation moved from wiki pages into the `docs` folder
- OAuth and nullable handling improvements across the codebase

### Removed

- Legacy FedEx SOAP implementation

### Breaking changes

- FedEx provider initialization changed to `FedExProviderConfiguration` with OAuth credentials
- See [Breaking changes](Breaking-Changes.md) for migration steps

## 3.0.0 (December 7, 2025)

### Added

- USPS REST API integration
- OAuth 2.0 support for USPS authentication

### Removed

- Legacy USPS Web Tools integration

### Breaking changes

- USPS initialization changed to `UspsProviderConfiguration`
- See [Breaking changes](Breaking-Changes.md) for migration steps
