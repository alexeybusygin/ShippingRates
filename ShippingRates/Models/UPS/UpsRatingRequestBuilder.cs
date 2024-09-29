using ShippingRates.ShippingProviders;
using System;
using System.Linq;

namespace ShippingRates.Models.UPS
{
    internal class UpsRatingRequestBuilder
    {
        private readonly UPSProviderConfiguration _configuration;

        public UpsRatingRequestBuilder(UPSProviderConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public UpsRatingRequest Build(Shipment shipment)
        {
            var shipFromUS = shipment.OriginAddress.CountryCode == "US";
            var unitsSystem = shipFromUS ? UnitsSystem.USCustomary : UnitsSystem.Metric;

            var request = new UpsRatingRequest()
            {
                RateRequest = new RateRequest()
                {
                    PickupType = new PickupType()
                    {
                        Code = "03"
                    },
                    Shipment = new UpsShipment()
                    {
                        PaymentDetails = new PaymentDetails()
                        {
                            ShipmentCharge = new ShipmentCharge()
                            {
                                BillShipper = new BillShipper()
                                {
                                    AccountNumber = _configuration.AccountNumber
                                },
                                Type = "01"
                            }
                        },
                        Shipper = new Shipper()
                        {
                            ShipperNumber = _configuration.AccountNumber,
                            Address = new UpsAddress(shipment.OriginAddress)
                        },
                        ShipFrom = new ShipAddress(new UpsAddress(shipment.OriginAddress)),
                        ShipTo = new ShipAddress(new UpsAddress(shipment.DestinationAddress)),
                        NumOfPieces = shipment.Packages.Count,
                        Package = shipment.Packages.Select(p => new UpsPackage(p, unitsSystem)).ToArray()
                    },
                    CustomerClassification = GetCustomerClassification()
                }
            };
            if (!string.IsNullOrEmpty(_configuration.ServiceDescription))
            {
                request.RateRequest.Shipment.Service = new Service()
                {
                    Code = GetServiceCode(_configuration.ServiceDescription)
                };
            }
            if (shipment.DestinationAddress.IsResidential)
            {
                request.RateRequest.Shipment.ShipTo.Address.ResidentialAddressIndicator = "Y";
            }
            if (shipment.HasDocumentsOnly)
            {
                request.RateRequest.Shipment.DocumentsOnlyIndicator = "Document";
            }
            if (shipment.Options.SaturdayDelivery)
            {
                request.RateRequest.Shipment.ShipmentServiceOptions = new ShipmentServiceOptions()
                {
                    SaturdayDeliveryIndicator = "Y"
                };
            }
            if (_configuration.UseNegotiatedRates)
            {
                request.RateRequest.Shipment.ShipmentRatingOptions = new ShipmentRatingOptions()
                {
                    NegotiatedRatesIndicator = "Y"
                };
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

        CustomerClassification GetCustomerClassification()
        {
            var customerClassification = _configuration.CustomerClassification;

            if (_configuration.UseRetailRates)
                customerClassification = UPSCustomerClassification.RetailRates;
            if (_configuration.UseDailyRates)
                customerClassification = UPSCustomerClassification.DailyRates;

            var code = ((int)customerClassification).ToString("D2");

            return new CustomerClassification()
            {
                Code = code,
            };
        }

        static string GetServiceCode(string serviceDescription)
        {
            if (serviceDescription.Length == 2)
                return serviceDescription;

            var serviceCode = UPSProvider.GetServiceCodes().FirstOrDefault(c => c.Value == serviceDescription).Key;

            if (string.IsNullOrEmpty(serviceCode))
                throw new ArgumentException($"Invalid UPS service description {serviceCode}");

            return serviceCode;
        }
    }
}
