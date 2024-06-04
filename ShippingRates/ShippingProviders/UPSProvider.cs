using ShippingRates.Models.UPS;
using ShippingRates.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShippingRates.ShippingProviders
{
    public class UPSProvider : AbstractShippingProvider
    {
        public override string Name => "UPS";

        readonly UPSProviderConfiguration _configuration;
        readonly HttpClient _httpClient;

        bool IsExternalHttpClient => _httpClient != null;

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
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public override async Task GetRates()
        {
            var httpClient = IsExternalHttpClient ? _httpClient : new HttpClient();

            try
            {
                var token = await UPSOAuthService.GetTokenAsync(_configuration, httpClient, AddError);

                if (!string.IsNullOrEmpty(token))
                {
                    var request = ShipmentToRequestAdapter.FromShipment(_configuration, Shipment);
                    var ratingsResponse = await UPSRatingService.GetRatingAsync(httpClient, token, _configuration.UseProduction, request, AddError);
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

        private void ParseResponse(UPSRatingResponse response)
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

        public static IDictionary<string, string> GetServiceCodes() => _serviceCodes;
    }
}
