using ShippingRates.Helpers.Json;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ShippingRates.ShippingProviders.Usps;

[JsonConverter(typeof(EnumMemberJsonConverter<UspsMailClass>))]
public enum UspsMailClass
{
    [EnumMember(Value = "ALL")]
    All,

    // Domestic Mail Classes
    [EnumMember(Value = "PARCEL_SELECT")]
    ParcelSelect,

    [EnumMember(Value = "PRIORITY_MAIL_EXPRESS")]
    PriorityMailExpress,

    [EnumMember(Value = "PRIORITY_MAIL")]
    PriorityMail,

    [EnumMember(Value = "LIBRARY_MAIL")]
    LibraryMail,

    [EnumMember(Value = "MEDIA_MAIL")]
    MediaMail,

    [EnumMember(Value = "BOUND_PRINTED_MATTER")]
    BoundPrintedMatter,

    [EnumMember(Value = "USPS_CONNECT_LOCAL")]
    UspsConnectLocal,

    [EnumMember(Value = "USPS_CONNECT_MAIL")]
    UspsConnectMail,

    [EnumMember(Value = "USPS_CONNECT_REGIONAL")]
    UspsConnectRegional,

    [EnumMember(Value = "USPS_GROUND_ADVANTAGE")]
    UspsGroundAdvantage,

    // International Mail Classes
    [EnumMember(Value = "FIRST-CLASS_PACKAGE_INTERNATIONAL_SERVICE")]
    FirstClassPackageInternationalService,

    [EnumMember(Value = "PRIORITY_MAIL_INTERNATIONAL")]
    PriorityMailInternational,

    [EnumMember(Value = "PRIORITY_MAIL_EXPRESS_INTERNATIONAL")]
    PriorityMailExpressInternational,

    [EnumMember(Value = "GLOBAL_EXPRESS_GUARANTEED")]
    GlobalExpressGuaranteed,
}
