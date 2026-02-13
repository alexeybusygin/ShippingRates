namespace ShippingRates.ShippingProviders.Usps;

/// <summary>
/// USPS Extra Service Codes
/// </summary>
public enum UspsExtraServiceCode
{
    Unknown = 0,

    /// <summary>USPS Delivery Duties Paid Fee</summary>
    DutiesPaidFee = 370,

    /// <summary>USPS Label Delivery Service</summary>
    LabelDeliveryService = 415,

    /// <summary>Tracking Plus 6 Months</summary>
    TrackingPlus6Months = 480,

    /// <summary>Tracking Plus 1 Year</summary>
    TrackingPlus1Year = 481,

    /// <summary>Tracking Plus 3 Years</summary>
    TrackingPlus3Years = 482,

    /// <summary>Tracking Plus 5 Years</summary>
    TrackingPlus5Years = 483,

    /// <summary>Tracking Plus 7 Years</summary>
    TrackingPlus7Years = 484,

    /// <summary>Tracking Plus 10 Years</summary>
    TrackingPlus10Years = 485,

    /// <summary>Tracking Plus Signature 3 Years</summary>
    TrackingPlusSignature3Years = 486,

    /// <summary>Tracking Plus Signature 5 Years</summary>
    TrackingPlusSignature5Years = 487,

    /// <summary>Tracking Plus Signature 7 Years</summary>
    TrackingPlusSignature7Years = 488,

    /// <summary>Tracking Plus Signature 10 Years</summary>
    TrackingPlusSignature10Years = 489,

    /// <summary>Hazardous Materials - Air Eligible Ethanol</summary>
    HazardousMaterialsAirEligibleEthanol = 810,

    /// <summary>Hazardous Materials - Class 1 – Toy Propellant/Safety Fuse Package</summary>
    HazardousMaterialsClass1 = 811,

    /// <summary>Hazardous Materials - Class 3 – Flammable and Combustible Liquids</summary>
    HazardousMaterialsClass3 = 812,

    /// <summary>Hazardous Materials - Class 7 – Radioactive Materials</summary>
    HazardousMaterialsClass7 = 813,

    /// <summary>Hazardous Materials - Class 8 – Air Eligible Corrosive Materials</summary>
    HazardousMaterialsClass8AirEligible = 814,

    /// <summary>Hazardous Materials - Class 8 – Nonspillable Wet Batteries</summary>
    HazardousMaterialsClass8NonspillableBatteries = 815,

    /// <summary>Hazardous Materials - Class 9 - Lithium Battery Marked Ground Only</summary>
    HazardousMaterialsClass9LithiumGroundOnly = 816,

    /// <summary>Hazardous Materials - Class 9 - Lithium Battery Returns</summary>
    HazardousMaterialsClass9LithiumReturns = 817,

    /// <summary>Hazardous Materials - Class 9 - Marked Lithium Batteries</summary>
    HazardousMaterialsClass9LithiumMarked = 818,

    /// <summary>Hazardous Materials - Class 9 – Dry Ice</summary>
    HazardousMaterialsClass9DryIce = 819,

    /// <summary>Hazardous Materials - Class 9 – Unmarked Lithium Batteries</summary>
    HazardousMaterialsClass9Lithium = 820,

    /// <summary>Hazardous Materials - Class 9 – Magnetized Materials</summary>
    HazardousMaterialsClass9Magnetized = 821,

    /// <summary>Hazardous Materials - Division 4.1 – Mailable Flammable Solids and Safety Matches</summary>
    HazardousMaterialsDivision41FlammableSolids = 822,

    /// <summary>Hazardous Materials - Division 5.1 – Oxidizers</summary>
    HazardousMaterialsDivision51Oxidizers = 823,

    /// <summary>Hazardous Materials - Division 5.2 – Organic Peroxides</summary>
    HazardousMaterialsDivision52OrganicPeroxides = 824,

    /// <summary>Hazardous Materials - Division 6.1 – Toxic Materials</summary>
    HazardousMaterialsDivision61Toxic = 825,

    /// <summary>Hazardous Materials - Division 6.2 Biological Materials</summary>
    HazardousMaterialsDivision62Biological = 826,

    /// <summary>Hazardous Materials - Excepted Quantity Provision</summary>
    HazardousMaterialsExceptedQuantity = 827,

    /// <summary>Hazardous Materials - Ground Only Hazardous Materials</summary>
    HazardousMaterialsGroundOnly = 828,

    /// <summary>Hazardous Materials - Air Eligible ID8000 Consumer Commodity</summary>
    HazardousMaterialsAirEligibleID8000 = 829,

    /// <summary>Hazardous Materials - Lighters</summary>
    HazardousMaterialsLighters = 830,

    /// <summary>Hazardous Materials - Limited Quantity Ground</summary>
    HazardousMaterialsLimitedQuantityGround = 831,

    /// <summary>Hazardous Materials - Small Quantity Provision (Markings Required)</summary>
    HazardousMaterialsSmallQuantityProvision = 832,

    /// <summary>Special Handling - Perishable Material</summary>
    SpecialHandlingPerishable = 853,

    /// <summary>Live Animal Transportation Fee</summary>
    LiveAnimalTransportationFee = 856,

    /// <summary>Hazardous Materials</summary>
    HazardousMaterials = 857,

    /// <summary>Cremated Remains</summary>
    CrematedRemains = 858,

    CertifiedMail = 910,
    CertifiedMailRestrictedDelivery = 911,
    CertifiedMailAdultSignatureRequired = 912,
    CertifiedMailAdultSignatureRestrictedDelivery = 913,

    CollectOnDelivery = 915,
    CollectOnDeliveryRestrictedDelivery = 917,

    USPSTrackingElectronic = 920,

    SignatureConfirmation = 921,
    AdultSignatureRequired = 922,
    AdultSignatureRestrictedDelivery = 923,
    SignatureConfirmationRestrictedDelivery = 924,

    PriorityMailExpressMerchandiseInsurance = 925,

    /// <summary>Insurance <= $500</summary>
    InsuranceUpTo500 = 930,

    /// <summary>Insurance > $500</summary>
    InsuranceOver500 = 931,

    InsuranceRestrictedDelivery = 934,

    RegisteredMail = 940,
    RegisteredMailRestrictedDelivery = 941,

    ReturnReceipt = 955,
    ReturnReceiptElectronic = 957,

    LiveAnimalAndPerishableHandlingFee = 972,

    SignatureRequestedPriorityMailExpressOnly = 981,

    ParcelLockerDelivery = 984,

    PoToAddresseePriorityMailExpressOnly = 986,

    SundayDelivery = 991
}
