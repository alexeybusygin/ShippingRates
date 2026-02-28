Service providers need some setup to return negotiated rates.

## FedEx

For FedEx, you need to set the `UseNegotiatedRates` flag:

```CSHARP
    rateManager.AddProvider(new FedExProvider(
        fedexKey,
        fedexPassword,
        fedexAccountNumber,
        fedexMeterNumber,
        fedexUseProduction)
    {
        UseNegotiatedRates = true
    });
```

## UPS

For UPS, you need to set the `UseNegotiatedRates` flag. Please pay attention that according to UPS documentation, only shippers approved to ship using negotiated rates can use negotiated rates.

```CSHARP
    rateManager.AddProvider(new UPSProvider(new UPSProviderConfiguration()
    {
        ClientId = upsClientId,
        ClientSecret = upsClientSecret,
        AccountNumber = upsAccountNumber,
        UseProduction = upsUseProduction,
        UseNegotiatedRates = true
    }));
```

## DHL

For DHL you need to provide `PaymentAccountNumber` in the `DHLProviderConfiguration`:

```CSHARP
    var dhlConfiguration = new DHLProviderConfiguration(dhlSiteId, dhlPassword, useProduction: true)
    {
       PaymentAccountNumber = "111"
    };
    rateManager.AddProvider(new DHLProvider(dhlConfiguration));
```

## USPS

USPS doesn't have negotiated rates but you can get the online discounted rates by setting the `Service` to `Services.Online`.

```CSHARP
    var configuration = new USPSProviderConfiguration(uspsUserId)
    {
        Service = Services.Online
    };
    rateManager.AddProvider(new USPSProvider(configuration));
```