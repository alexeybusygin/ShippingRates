using ShippingRates.Models.OAuth;

namespace ShippingRates.ShippingProviders.Usps;

/// <summary>
/// Configuration for the USPS provider.
/// </summary>
public class UspsProviderConfiguration : IOAuthConfiguration
{
    /// <summary>
    /// USPS API client id (OAuth).
    /// Required.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// USPS API client secret (OAuth).
    /// Required.
    /// </summary>
    public string? ClientSecret { get; set; }

    public bool UseProduction { get; set; }

    /// <summary>
    /// Pricing type requested from USPS.
    /// 
    /// COMMERCIAL is the default and should be used in almost all cases.
    /// Changing this does not guarantee availability of rates;
    /// it only influences which price tables USPS may consider.
    /// </summary>
    public UspsPriceType PriceType { get; set; } = UspsPriceType.COMMERCIAL;

    /// <summary>
    /// Preferred processing category.
    /// </summary>
    public UspsProcessingCategory ProcessingCategory { get; set; } = UspsProcessingCategory.MACHINABLE;

    /// <summary>
    /// Mail classes to consider during rate search.
    /// 
    /// Using <see cref="UspsMailClass.All"/> means USPS will return only
    /// mail classes that fully qualify for the shipment.
    /// Some services (e.g. First-Class, insurance) may disappear silently
    /// if package weight, shape, or extras are incompatible.
    /// 
    /// For requests with extra services (e.g. insurance), it is strongly
    /// recommended to specify mail classes explicitly (e.g. Priority Mail).
    /// </summary>
    public UspsMailClass[] MailClasses { get; set; } = [UspsMailClass.All];

    /// <summary>
    /// Extra services requested (e.g. insurance, signature confirmation).
    /// </summary>
    public UspsExtraServiceCode[] ExtraServiceCodes { get; set; } = [];

    /// <summary>
    /// USPS account type (if required for specific commercial contracts).
    /// Optional for most standard integrations.
    /// </summary>
    public string? AccountType { get; set; }

    /// <summary>
    /// USPS account number associated with the commercial pricing agreement.
    /// Optional unless required by USPS for certain rate or service combinations.
    /// </summary>
    public string? AccountNumber { get; set; }
}
