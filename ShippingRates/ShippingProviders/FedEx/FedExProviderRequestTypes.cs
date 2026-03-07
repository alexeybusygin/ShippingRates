namespace ShippingRates.ShippingProviders.FedEx;

public enum FedExPickupType
{
    ContactFedExToSchedule = 0,
    DropoffAtFedExLocation = 1,
    UseScheduledPickup = 2
}

public enum FedExPackagingType
{
    YourPackaging = 0,
    FedExEnvelope = 1,
    FedExPak = 2,
    FedExBox = 3,
    FedExTube = 4,
    FedEx10KgBox = 5,
    FedEx25KgBox = 6,
    FedExSmallBox = 7,
    FedExMediumBox = 8,
    FedExLargeBox = 9,
    FedExExtraLargeBox = 10
}
