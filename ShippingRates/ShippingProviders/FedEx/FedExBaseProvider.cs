using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ShippingRates.ShippingProviders.FedEx;

public abstract class FedExBaseProvider<T> : AbstractShippingProvider where T : FedExBaseProvider<T>
{
    protected abstract Dictionary<string, string> ServiceCodes { get; }

    protected readonly FedExProviderConfiguration _configuration;
    protected readonly ILogger<T>? _logger;

    public FedExBaseProvider(FedExProviderConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        if (string.IsNullOrEmpty(_configuration.ClientId))
            throw new Exception("ClientId is required");
        if (string.IsNullOrEmpty(_configuration.ClientSecret))
            throw new Exception("ClientSecret is required");
    }

    public FedExBaseProvider(FedExProviderConfiguration configuration, HttpClient httpClient)
        : this(configuration)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public FedExBaseProvider(FedExProviderConfiguration configuration, ILogger<T> logger)
        : this(configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public FedExBaseProvider(FedExProviderConfiguration configuration, HttpClient httpClient, ILogger<T> logger)
        : this(configuration, httpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     FedEx allows insured values for items being shipped except when utilizing SmartPost.
    ///     This setting will this value to be overwritten.
    /// </summary>
    protected bool _allowInsuredValues = true;

    /// <summary>
    /// Gets service codes.
    /// </summary>
    /// <returns></returns>
    public IDictionary<string, string> GetServiceCodes() => ServiceCodes;

    public static string GetRequestUri(bool isProduction)
        => $"https://{(isProduction ? "apis" : "apis-sandbox")}.fedex.com";

}
