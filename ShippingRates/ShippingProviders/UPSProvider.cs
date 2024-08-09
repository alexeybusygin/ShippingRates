using ShippingRates.Models;
using ShippingRates.Models.UPS;
using ShippingRates.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShippingRates.ShippingProviders
{
    public class UPSProvider : AbstractShippingProvider, IAddressValidator
    {
        public override string Name => "UPS";

        readonly UPSProviderConfiguration _configuration;

        readonly static Dictionary<string, string> _serviceCodes = new Dictionary<string, string>()
        {
            { "01", "UPS Next Day Air" },
            { "02", "UPS Second Day Air" },
            { "03", "UPS Ground" },
            { "07", "UPS Worldwide Express" },
            { "08", "UPS Worldwide Expedited" },
            { "11", "UPS Standard" },
            { "12", "UPS 3-Day Select" },
            { "13", "UPS Next Day Air Saver" },
            { "14", "UPS Next Day Air Early AM" },
            { "54", "UPS Worldwide Express Plus" },
            { "59", "UPS 2nd Day Air AM" },
            { "65", "UPS Express Saver" },
            { "71", "UPS Worldwide Express Freight Midday" },
            { "75", "UPS Heavy Goods" },
            { "93", "UPS Sure Post" },
            { "96", "UPS Worldwide Express Freight" }
        };

        public UPSProvider(UPSProviderConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if (string.IsNullOrEmpty(_configuration.ClientId))
                throw new Exception("ClientId is required");
            if (string.IsNullOrEmpty(_configuration.ClientSecret))
                throw new Exception("ClientSecret is required");
            if (string.IsNullOrEmpty(_configuration.AccountNumber))
                throw new Exception("AccountNumber is required");
        }

        public UPSProvider(UPSProviderConfiguration configuration, HttpClient httpClient)
            : this(configuration)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public override async Task GetRates()
        {
            var httpClient = IsExternalHttpClient ? HttpClient : new HttpClient();

            try
            {
                var token = await UPSOAuthService.GetTokenAsync(_configuration, httpClient, AddError);

                if (!string.IsNullOrEmpty(token))
                {
                    var request = GetRequest();
                    var ratingsResponse = await UpsRatingService.GetRatingAsync(httpClient, token, _configuration.UseProduction, request, AddError);
                    ParseResponse(ratingsResponse);
                }
            }
            catch (Exception e)
            {
                AddInternalError($"UPS Provider Exception: {e.Message}");
            }
            finally
            {
                if (!IsExternalHttpClient && httpClient != null)
                    httpClient.Dispose();
            }
        }

        private UpsRatingRequest GetRequest()
        {
            var shipFromUS = Shipment.OriginAddress.CountryCode == "US";
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
                            Address = new UpsAddress(Shipment.OriginAddress)
                        },
                        ShipFrom = new ShipAddress(new UpsAddress(Shipment.OriginAddress)),
                        ShipTo = new ShipAddress(new UpsAddress(Shipment.DestinationAddress)),
                        NumOfPieces = Shipment.Packages.Count,
                        Package = Shipment.Packages.Select(p => new UpsPackage(p, unitsSystem)).ToArray()
                    }
                }
            };
            if (!string.IsNullOrEmpty(_configuration.ServiceDescription))
            {
                request.RateRequest.Shipment.Service = new Service()
                {
                    Code = GetServiceCode(_configuration.ServiceDescription)
                };
            }
            if (Shipment.DestinationAddress.IsResidential)
            {
                request.RateRequest.Shipment.ShipTo.Address.ResidentialAddressIndicator = "Y";
            }
            if (Shipment.HasDocumentsOnly)
            {
                request.RateRequest.Shipment.DocumentsOnlyIndicator = "Document";
            }
            if (Shipment.Options.SaturdayDelivery)
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
            if (shipFromUS)         // Valid if ship from US
            {
                var code = _configuration.UseRetailRates
                    ? "04"
                    : (_configuration.UseDailyRates ? "01" : "00");
            }
            if (Shipment.Options.ShippingDate != null)
            {
                request.RateRequest.Shipment.DeliveryTimeInformation = new DeliveryTimeInformation()
                {
                    PackageBillType = Shipment.HasDocumentsOnly ? "02" : "03",
                    Pickup = new Pickup()
                    {
                        Date = Shipment.Options.ShippingDate.Value.ToString("yyyyMMdd"),
                        Time = "1000"
                    }
                };
            }

            return request;
        }

        private void ParseResponse(UpsRatingResponse response)
        {
            if (response?.RateResponse?.RatedShipment == null)
                return;

            foreach (var rate in response.RateResponse.RatedShipment)
            {
                var serviceCode = rate.Service.Code;
                if (!_serviceCodes.ContainsKey(serviceCode))
                {
                    AddInternalError($"Unknown service code {serviceCode}");
                    continue;
                }
                var serviceDescription = _serviceCodes[serviceCode];

                var totalCharges = Convert.ToDecimal(rate.TotalCharges.MonetaryValue);
                var currencyCode = rate.TotalCharges.CurrencyCode;

                if (_configuration.UseNegotiatedRates && rate.NegotiatedRateCharges != null)
                {
                    totalCharges = Convert.ToDecimal(rate.NegotiatedRateCharges.TotalCharge.MonetaryValue);
                    currencyCode = rate.NegotiatedRateCharges.TotalCharge.CurrencyCode;
                }

                // Use MaxDate as default to ensure correct sorting
                var estDeliveryDate = DateTime.MaxValue.ToShortDateString();;
                var businessDaysInTransit = rate.GuaranteedDelivery?.BusinessDaysInTransit;
                if (!string.IsNullOrEmpty(businessDaysInTransit))
                {
                    estDeliveryDate = (Shipment.Options.ShippingDate ?? DateTime.Now)
                        .AddDays(Convert.ToDouble(businessDaysInTransit)).ToShortDateString();
                }
                var deliveryTime = rate.GuaranteedDelivery?.DeliveryByTime;
                if (string.IsNullOrEmpty(deliveryTime)) // No scheduled delivery time, so use 11:59:00 PM to ensure correct sorting
                {
                    estDeliveryDate += " 11:59:00 PM";
                }
                else
                {
                    estDeliveryDate += " " + deliveryTime.Replace("Noon", "PM").Replace("P.M.", "PM").Replace("A.M.", "AM");
                }
                var deliveryDate = DateTime.Parse(estDeliveryDate);

                AddRate(serviceCode, serviceDescription, totalCharges, deliveryDate, new RateOptions()
                {
                    SaturdayDelivery = Shipment.Options.SaturdayDelivery && deliveryDate.DayOfWeek == DayOfWeek.Saturday
                }, currencyCode);
            }
        }

        static string GetServiceCode(string serviceDescription)
        {
            if (serviceDescription.Length == 2)
                return serviceDescription;

            var serviceCode = _serviceCodes.FirstOrDefault(c => c.Value == serviceDescription).Key;

            if (string.IsNullOrEmpty(serviceCode))
                throw new ArgumentException($"Invalid UPS service description {serviceCode}");

            return serviceCode;
        }

        public static IDictionary<string, string> GetServiceCodes() => _serviceCodes;

        public async Task<AddressValidationResult> ValidateAddressAsync(Address address)
        {
            var httpClient = IsExternalHttpClient ? HttpClient : new HttpClient();
            var result = new AddressValidationResult()
            {
                IsValid = false
            };

            try
            {
                var token = await UPSOAuthService.GetTokenAsync(_configuration, httpClient, AddAddressError);

                if (!string.IsNullOrEmpty(token))
                {
                    var request = new UpsAddressValidationRequest(address);
                    var ratingsResponse = await UpsAddressValidationService.ValidateAsync(httpClient, token, _configuration.UseProduction, request, AddAddressError);
                    ParseResponse(ratingsResponse);
                    return new AddressValidationResult();
                }
            }
            catch (Exception e)
            {
                result.InternalErrors.Add($"UPS Provider Exception: {e.Message}");
            }
            finally
            {
                if (!IsExternalHttpClient && httpClient != null)
                    httpClient.Dispose();
            }

            void AddAddressError(Error error)
            {
                result.Errors.Add(error);
            }

            return result;
        }
    }
}
