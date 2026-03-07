using System.Net.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShippingRates.ShippingProviders;

/// <summary>
///     A base implementation of the <see cref="IShippingProvider" /> interface.
///     All provider-specific classes should inherit from this class.
/// </summary>
public abstract class AbstractShippingProvider : IShippingProvider
{
    public abstract Task<RateResult> GetRatesAsync(Shipment shipment, CancellationToken cancellationToken = default);
    public abstract string Name { get; }

    private HttpClient? _externalHttpClient;

    protected void SetHttpClient(HttpClient httpClient)
    {
        _externalHttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    protected virtual HttpClient CreateInternalHttpClient() => new();

    protected HttpClientLease RentHttpClient()
    {
        if (_externalHttpClient != null)
        {
            return new HttpClientLease(_externalHttpClient, ownsClient: false);
        }

        return new HttpClientLease(CreateInternalHttpClient(), ownsClient: true);
    }

    protected readonly struct HttpClientLease : IDisposable
    {
        public HttpClient HttpClient { get; }
        private readonly bool _ownsClient;

        public HttpClientLease(HttpClient httpClient, bool ownsClient)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _ownsClient = ownsClient;
        }

        public void Dispose()
        {
            if (_ownsClient)
            {
                HttpClient.Dispose();
            }
        }
    }
}
