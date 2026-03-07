using ShippingRates.Models.OAuth;

namespace ShippingRates.ShippingProviders.FedEx;

public class FedExProviderConfiguration : IOAuthConfiguration
{
    /// <summary>
    /// FedEx Client Id.
    /// Required.
    /// </summary>
    public string? ClientId { get; set; }
    /// <summary>
    /// FedEx Client Secret.
    /// Required.
    /// </summary>
    public string? ClientSecret { get; set; }
    /// <summary>
    /// FedEx Account Number
    /// </summary>
    public string? AccountNumber { get; set; }
    public bool UseProduction { get; set; }
    public bool UseNegotiatedRates { get; set; } = false;
    /// <summary>
    /// Pickup type.
    /// </summary>
    public FedExPickupType PickupType { get; set; } = FedExPickupType.UseScheduledPickup;
    /// <summary>
    /// Packaging type.
    /// </summary>
    public FedExPackagingType PackagingType { get; set; } = FedExPackagingType.YourPackaging;
    /// <summary>
    /// Hub ID for FedEx SmartPost.
    /// If not using the production Rate API, you can use 5531 as the HubID per FedEx documentation.
    /// </summary>
    public string? HubId { get; set; }
}
