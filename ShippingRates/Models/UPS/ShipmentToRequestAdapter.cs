using ShippingRates.ShippingProviders;
using System;
using System.Linq;

namespace ShippingRates.Models.UPS
{
    internal class ShipmentToRequestAdapter
    {
        public static UPSRatingRequest FromShipment(UPSProviderConfiguration configuration, ShippingRates.Shipment shipment)
        {
            var request = new UPSRatingRequest()
            {
                RateRequest = new RateRequest()
                {
                    PickupType = new PickupType()
                    {
                        Code = "03"
                    },
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
                    Code = GetServiceCode(configuration.ServiceDescription)
                };
            }
            if (shipment.DestinationAddress.IsResidential)
            {
                request.RateRequest.Shipment.ShipTo.Address.ResidentialAddressIndicator = "True";
            }
            if (shipment.HasDocumentsOnly)
            {
                request.RateRequest.Shipment.DocumentsOnlyIndicator = "Document";
            }
            if (shipment.Options.SaturdayDelivery)
            {
                request.RateRequest.Shipment.ShipmentServiceOptions = new ShipmentServiceOptions()
                {
                    SaturdayDeliveryIndicator = "True"
                };
            }
            if (configuration.UseNegotiatedRates)
            {
                request.RateRequest.Shipment.ShipmentRatingOptions = new ShipmentRatingOptions()
                {
                    NegotiatedRatesIndicator = "True"
                };
            }
            if (shipment.OriginAddress.CountryCode == "US")         // Valid if ship from US
            {
                var code = configuration.UseRetailRates
                    ? "04"
                    : (configuration.UseDailyRates ? "01" : "00");
            }
            if (shipment.Options.ShippingDate != null)
            {
                request.RateRequest.Shipment.DeliveryTimeInformation = new DeliveryTimeInformation()
                {
                    PackageBillType = shipment.HasDocumentsOnly ? "02" : "03",
                    Pickup = new Pickup()
                    {
                        Date = shipment.Options.ShippingDate.Value.ToString("yyyyMMdd"),
                        Time = "1000"
                    }
                };
            }

            return request;
        }

        static string GetServiceCode(string serviceDescription)
        {
            if (serviceDescription.Length == 2)
                return serviceDescription;

            var serviceCode = UPSProvider.GetServiceCodes()
                .FirstOrDefault(c => c.Value == serviceDescription).Key;

            if (string.IsNullOrEmpty(serviceCode))
                throw new ArgumentException($"Invalid UPS service description {serviceCode}");

            return serviceCode;
        }

        static Package FromPackage(ShippingRates.Package package)
        {
            var ratesPackage = new Package()
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

            if (package.SignatureRequiredOnDelivery)
            {
                ratesPackage.PackageServiceOptions = new PackageServiceOptions()
                {
                    DeliveryConfirmation = new DeliveryConfirmation()
                    {
                        DCISType = "2"
                    }
                };
            }

            return ratesPackage;
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
}
