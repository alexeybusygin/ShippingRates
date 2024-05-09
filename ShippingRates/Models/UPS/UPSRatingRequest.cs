using System;

namespace ShippingRates.Models.UPS
{
    internal class UPSRatingRequest
    {
        public RateRequest RateRequest { get; set; }
    }

    internal class RateRequest
    {
        public TransactionReference TransactionReference { get; set; }
        public PickupType PickupType { get; set; }
        public CustomerClassification CustomerClassification { get; set; }
        public Shipment Shipment { get; set; }
    }

    internal class TransactionReference
    {
        public string CustomerContext { get; set; }
    }

    internal class PickupType
    {
        public string Code { get; set; }
    }

    internal class CustomerClassification
    {
        public string Code { get; set; }
    }

    internal class Shipment
    {
        public Shipper Shipper { get; set; }
        public ShipAddress ShipFrom { get; set; }
        public ShipAddress ShipTo { get; set; }
        public PaymentDetails PaymentDetails { get; set; }
        public Service Service { get; set; }
        public int NumOfPieces { get; set; }
        public string DocumentsOnlyIndicator { get; set; }
        public Package[] Package { get; set; }
        public ShipmentServiceOptions ShipmentServiceOptions { get; set; }
        public ShipmentRatingOptions ShipmentRatingOptions { get; set; }
        public DeliveryTimeInformation DeliveryTimeInformation { get; set; }
    }

    internal class Shipper
    {
        public string Name { get; set; }
        public string ShipperNumber { get; set; }
        public Address Address { get; set; }
    }

    internal class ShipAddress
    {
        public string Name { get; set; }
        public Address Address { get; set; }
    }

    internal class Address
    {
        public string[] AddressLine = Array.Empty<string>();
        public string City { get; set; }
        public string StateProvinceCode { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
        public string ResidentialAddressIndicator { get; set; }
    }

    internal class Service
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }

    internal class PaymentDetails
    {
        public ShipmentCharge ShipmentCharge { get; set; }
    }

    internal class ShipmentCharge
    {
        public string Type { get; set; }
        public BillShipper BillShipper { get; set; }
    }

    internal class BillShipper
    {
        public string AccountNumber { get; set; }
    }

    internal class Package
    {
        public PackagingType PackagingType { get; set; }
        public PackageWeight PackageWeight { get; set; }
        public Dimensions Dimensions { get; set; }
        public PackageServiceOptions PackageServiceOptions  { get; set; }
    }

    internal class Dimensions
    {
        public UnitOfMeasurement UnitOfMeasurement { get; set; }
        public string Length { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
    }

    internal class PackageServiceOptions
    {
        public DeliveryConfirmation DeliveryConfirmation { get; set; }
    }

    internal class DeliveryConfirmation
    {
        public string DCISType { get; set; }
    }

    internal class PackageWeight
    {
        public UnitOfMeasurement UnitOfMeasurement { get; set; }
        public string Weight { get; set; }
    }

    internal class UnitOfMeasurement
    {
        public string Code { get; set; }
    }

    internal class PackagingType
    {
        public string Code { get; set; }
    }

    internal class ShipmentServiceOptions
    {
        public string SaturdayDeliveryIndicator { get; set; }
    }

    internal class ShipmentRatingOptions
    {
        public string NegotiatedRatesIndicator { get; set; }
    }

    internal class DeliveryTimeInformation
    {
        public string PackageBillType { get; set; }
        public Pickup Pickup { get; set; }
    }

    internal class Pickup
    {
        public string Date { get; set; }
        public string Time { get; set; }
    }
}
