using Microsoft.Extensions.Configuration;

namespace ShippingRates.Tests;

public class ConfigHelper
{
    public static IConfigurationRoot GetConfigurationRoot(string outputPath)
    {
        return new ConfigurationBuilder()
            .SetBasePath(outputPath)                            // For local testing
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables("ShippingRates.")          // For CI
            .Build();
    }

    public static TestsConfiguration GetApplicationConfiguration(string outputPath)
    {
        var configurationRoot = GetConfigurationRoot(outputPath);

        var configuration = new TestsConfiguration();

        configurationRoot.Bind(configuration);

        return configuration;
    }
}

