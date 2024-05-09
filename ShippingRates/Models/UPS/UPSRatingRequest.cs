using ShippingRates.ShippingProviders;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;

namespace ShippingRates.Models.UPS
{
    internal class UPSRatingRequest
    {
        public RateRequest RateRequest { get; set; }

        public static UPSRatingRequest FromShipment(UPSProviderConfiguration configuration, ShippingRates.Shipment shipment)
        {
            var request = new UPSRatingRequest()
            {
                RateRequest = new RateRequest()
                {
                    Shipment = new Shipment()
                    {
                        PaymentDetails = new PaymentDetails()
                        {
                            ShipmentCharge = new ShipmentCharge()
                            {
                                BillShipper = new BillShipper()
                                {
                                    AccountNumber = configuration.AccountNumber
                                },
                                Type = "01"
                            }
                        },
                        Shipper = new Shipper()
                        {
                            ShipperNumber = configuration.AccountNumber,
                            Address = FromAddress(shipment.OriginAddress)
                        },
                        ShipFrom = new ShipAddress()
                        {
                            Address = FromAddress(shipment.OriginAddress)
                        },
                        ShipTo = new ShipAddress()
                        {
                            Address = FromAddress(shipment.DestinationAddress)
                        },
                        NumOfPieces = shipment.Packages.Count,
                        Package = shipment.Packages.Select(p => FromPackage(p)).ToArray()
                    }
                }
            };
            if (!string.IsNullOrEmpty(configuration.ServiceDescription))
            {
                request.RateRequest.Shipment.Service = new Service()
                {
                    Code = configuration.ServiceDescription.ToUpsShipCode()
                };
            }

            return request;
        }

        static Package FromPackage(ShippingRates.Package package)
        {
            return new Package()
            {
                PackagingType = new PackagingType()
                {
                    Code = "02"
                },
                PackageWeight = new PackageWeight()
                {
                    UnitOfMeasurement = new UnitOfMeasurement()
                    {
                        Code = "LBS"
                    },
                    Weight = package.RoundedWeight.ToString()
                },
                Dimensions = new Dimensions()
                {
                    UnitOfMeasurement = new UnitOfMeasurement()
                    {
                        Code = "IN"
                    },
                    Length = package.RoundedLength.ToString(),
                    Width = package.RoundedWidth.ToString(),
                    Height = package.RoundedHeight.ToString()
                }
            };
        }

        static Address FromAddress(ShippingRates.Address address)
        {
            var addressLines = new string[] { address.Line1, address.Line2, address.Line3 }
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            return new Address()
            {
                AddressLine = addressLines,
                City = address.City,
                StateProvinceCode = address.State,
                PostalCode = address.PostalCode,
                CountryCode = address.CountryCode
            };
        }
    }

    internal class RateRequest
    {
        //public TransactionReference TransactionReference { get; set; }
        public Shipment Shipment { get; set; }
    }

    internal class TransactionReference
    {
        public string CustomerContext { get; set; }
    }

    internal class Shipment
    {
        public Shipper Shipper { get; set; }
        public ShipAddress ShipFrom { get; set; }
        public ShipAddress ShipTo { get; set; }
        public PaymentDetails PaymentDetails { get; set; }
        public Service Service { get; set; }
        public int NumOfPieces { get; set; }
        public Package[] Package { get; set; }
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
    }

    /*
     * Service: {
              Code: '03',
              Description: 'Ground'
            }
     */
    internal class Service
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }

    /*
     * PaymentDetails: {
              ShipmentCharge: {
                Type: '01',
                BillShipper: {
                  AccountNumber: 'G66641'
                }
              }
            }
     */
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

    /*
     * Package: {
              SimpleRate: {
                Description: 'SimpleRateDescription',
                Code: 'XS'
              },
              PackagingType: {
                Code: '02',
                Description: 'Packaging'
              },
              Dimensions: {
                UnitOfMeasurement: {
                  Code: 'IN',
                  Description: 'Inches'
                },
                Length: '5',
                Width: '5',
                Height: '5'
              },
              PackageWeight: {
                UnitOfMeasurement: {
                  Code: 'LBS',
                  Description: 'Pounds'
                },
                Weight: '1'
              }
            }
     */
    internal class Package
    {
        public PackagingType PackagingType { get; set; }
        public PackageWeight PackageWeight { get; set; }
        public Dimensions Dimensions { get; set; }
    }

    internal class Dimensions
    {
        public UnitOfMeasurement UnitOfMeasurement { get; set; }
        public string Length { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
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

    /*var json = @"{
        RateRequest: {
          Request: {
            TransactionReference: {
              CustomerContext: 'CustomerContext'
            }
          },
          Shipment: {
            Shipper: {
              Name: 'ShipperName',
              ShipperNumber: 'G66641',
              Address: {
                AddressLine: [
                  'ShipperAddressLine',
                  'ShipperAddressLine',
                  'ShipperAddressLine'
                ],
                City: 'TIMONIUM',
                StateProvinceCode: 'MD',
                PostalCode: '21093',
                CountryCode: 'US'
              }
            },
            ShipTo: {
              Name: 'ShipToName',
              Address: {
                AddressLine: [
                  'ShipToAddressLine',
                  'ShipToAddressLine',
                  'ShipToAddressLine'
                ],
                City: 'Alpharetta',
                StateProvinceCode: 'GA',
                PostalCode: '30005',
                CountryCode: 'US'
              }
            },
            ShipFrom: {
              Name: 'ShipFromName',
              Address: {
                AddressLine: [
                  'ShipFromAddressLine',
                  'ShipFromAddressLine',
                  'ShipFromAddressLine'
                ],
                City: 'TIMONIUM',
                StateProvinceCode: 'MD',
                PostalCode: '21093',
                CountryCode: 'US'
              }
            },
            ,
            ,
            NumOfPieces: '1',
            
          }
        }
      }";*/
}
