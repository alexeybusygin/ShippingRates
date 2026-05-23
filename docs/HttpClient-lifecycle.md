The recommended approach is to pass a reusable `HttpClient` instance as a parameter for shipping provider constructors:

```CSHARP
using var httpClient = new HttpClient();

// UPS
rateManager.AddProvider(new UPSProvider(new UPSProviderConfiguration {...}, httpClient));

// USPS
rateManager.AddProvider(new UspsProvider(new UspsProviderConfiguration {...}, httpClient));

// DHL
rateManager.AddProvider(new DHLProvider(new DHLProviderConfiguration {...}, httpClient));
```

This way, `HttpClient` can be reusable, and you can get the full advantages of managing the lifecycle on your own, especially if you use `HttpClientFactory`.

If you supply one for FedEx, configure automatic decompression so compressed FedEx responses can be parsed correctly:

```csharp
using System.Net;
using System.Net.Http;

var handler = new HttpClientHandler
{
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
};

using var httpClient = new HttpClient(handler);

rateManager.AddProvider(new FedExProvider(fedExConfiguration, httpClient));
```

Shipping providers don't implement IDisposable for backward compatibility, so if `HttpClient `is not passed as a parameter, then a new instance will be created for each `GetRates` call of each provider added to the `RateManager`.
