namespace ShippingRates.IntegrationTests;

public class TestsConfiguration
{
    public string? USPSClientId { get; set; }
    public string? USPSClientSecret { get; set; }
    public bool USPSUseProduction { get; set; }

    public string? UPSAccountNumber { get; set; }
    public string? UPSClientId { get; set; }
    public string? UPSClientSecret { get; set; }
    public bool UPSUseProduction { get; set; }

    public string? FedExClientId { get; set; }
    public string? FedExClientSecret { get; set; }
    public string? FedExAccountNumber { get; set; }
    public string? FedExHubId { get; set; }
    public bool FedExUseProduction { get; set; }

    public string? DHLSiteId { get; set; }
    public string? DHLPassword { get; set; }
    public string? DHLAccountNumber { get; set; }
}
