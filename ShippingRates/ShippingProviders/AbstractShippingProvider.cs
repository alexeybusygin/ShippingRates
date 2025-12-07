using System.Net.Http;
using System.Threading.Tasks;

namespace ShippingRates.ShippingProviders;

/// <summary>
///     A base implementation of the <see cref="IShippingProvider" /> interface.
///     All provider-specific classes should inherit from this class.
/// </summary>
public abstract class AbstractShippingProvider : IShippingProvider
{
    public abstract Task<RateResult> GetRatesAsync(Shipment shipment);
    public abstract string Name { get; }

    private HttpClient? _httpClient;
    protected HttpClient? HttpClient
    {
        get => _httpClient;
        set
        {
            _httpClient = value;
            IsExternalHttpClient = _httpClient != null;
        }
    }
    protected bool IsExternalHttpClient { get; private set; }
}
