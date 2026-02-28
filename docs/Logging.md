The UPS shipping provider supports logging via `ILogger` (other providers' support is in progress). You can pass an `ILogger` instance using the corresponding constructor:

```c#
public UPSProvider(UPSProviderConfiguration configuration, ILogger<UPSProvider> logger);
public UPSProvider(UPSProviderConfiguration configuration, HttpClient httpClient, ILogger<UPSProvider> logger);
```

### Logging Levels

UPSProvider logs information at different levels:

* Error – Errors and exceptions
* Information – JSON requests and responses
* Debug – Additional details (e.g., acquiring a token)

### Sensitive Data in Logs

🚨 **Important:** Some sensitive information is included in JSON requests and may require filtering on your side:
* UPS Account Number.
* Billings Address.
* Ship From Address.
* Ship To Address.

Consider using structured logging with a filtering mechanism, such as Serilog with `Destructure.ByTransforming`, to prevent sensitive data from being logged.

### Using Logging Without Dependency Injection

If you are not using dependency injection, you can create an `ILogger` instance using `LoggerFactory`:

```c#
using (var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole(); // Adjust output as needed
}))
{
    var upsProviderLogger = loggerFactory.CreateLogger<UPSProvider>();
    rateManager.AddProvider(new UPSProvider(upsConfiguration, httpClient, upsProviderLogger));
    ...
}
```
