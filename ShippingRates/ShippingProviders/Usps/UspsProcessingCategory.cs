namespace ShippingRates.ShippingProviders.Usps;

public enum UspsProcessingCategory
{
    /// <summary>
    /// Fully automated processing (letters and most boxes).
    /// </summary>
    MACHINABLE,

    /// <summary>
    /// Large envelopes/flats (magazines, catalogs).
    /// </summary>
    FLATS,

    /// <summary>
    /// Nonstandard size/shape (often BPM, odd parcels).
    /// </summary>
    NONSTANDARD,
}
