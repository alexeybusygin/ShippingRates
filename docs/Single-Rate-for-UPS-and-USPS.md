ShippingRates supports requesting a single specific rate from both UPS and USPS. To do so, provide the desired service or mail class in the provider configuration when creating the provider instance.

```CSHARP
// UPS: request a specific service (e.g., "UPS Ground")
var upsConfiguration = new UPSProviderConfiguration()
{
    ClientId = upsClientId,
    ClientSecret = upsClientSecret,
    AccountNumber = upsAccountNumber,
    UseProduction = false,
    ServiceDescription = "UPS Ground",
};

rateManager.AddProvider(new UPSProvider(upsConfiguration, httpClient));
````

```CSHARP
// USPS: request a specific mail class
var uspsConfiguration = new UPSProviderConfiguration()
{
    ClientId = uspsClientId,
    ClientSecret = uspsClientSecret,
    UseProduction = false,
    MailClasses = [UspsMailClass.PriorityMail]
};

rateManager.AddProvider(new USPSProvider(uspsConfiguration, httpClient));
````

A list of valid UPS service descriptions can be found [here](https://github.com/alexeybusygin/ShippingRates/blob/5a0257fd35bfaeef0911609fb456cf8b1b8f569f/ShippingRates/ShippingProviders/UPSProvider.cs#L18).

A list of valid USPS mail classes can be found [here](https://github.com/alexeybusygin/ShippingRates/blob/b880d05773d7dcb1948de1499dc0d91ef2a999d6/ShippingRates/ShippingProviders/Usps/UspsMailClass.cs#L8).