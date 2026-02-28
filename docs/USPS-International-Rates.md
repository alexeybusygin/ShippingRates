**UPDATE**: This page is obsolete. Starting with v3.0.0, USPS domestic and international shipments are handled by a single unified `UspsProvider`.

USPS requires a separate API call to retrieve rates for international services. The call works the same way, but use `USPSInternationalProvider` instead. Your current USPS credentials will work with this and will return the available services between the origin and destination addresses:

```CSHARP
    using var httpClient = new HttpClient();
    var rateManager = new RateManager();
    var upsConfiguration = new USPSProviderConfiguration(uspsUserId);
    rateManager.AddProvider(new USPSInternationalProvider(upsConfiguration, httpClient));
```

**NOTE**: This behavior will be changed in the future in favor of using a single `USPSProvider`.