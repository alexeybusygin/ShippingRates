The recommended approach is to pass the `HttpClient` instance as a parameter for shipping provider constructors:

```CSHARP
    using (var httpClient = new HttpClient())

    // UPS International
    rateManager.AddProvider(new UPSProvider(upsConfiguration, httpClient));
    
    // USPS Domestic
    rateManager.AddProvider(new USPSProvider(new USPSProviderConfiguration(uspsUserId), httpClient));
    // USPS International
    rateManager.AddProvider(new USPSInternationalProvider(new USPSProviderConfiguration(uspsUserId), httpClient));

    // DHL
    rateManager.AddProvider(new DHLProvider(dhlConfiguration, httpClient));
```

This way, `HttpClient` can be reusable, and you can get the full advantages of managing the lifecycle on your own, especially if you use `HttpClientFactory`. That works for UPS, DHL, and USPS. It doesn't work for FedEx because it uses an old SOAP implementation. But after switching FedEx to REST OAuth API the same approach will be available as well.

Shipping providers don't implement IDisposable for backward compatibility, so if `HttpClient `is not passed as a parameter, then a new instance will be created for each `GetRates` call of each provider added to the `RateManager`.