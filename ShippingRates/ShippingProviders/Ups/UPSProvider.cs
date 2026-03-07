using Microsoft.Extensions.Logging;
using ShippingRates.Models;
using ShippingRates.Models.Ups;
using ShippingRates.Services.Ups;
using ShippingRates.ShippingProviders.Ups;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ShippingRates.ShippingProviders;

public class UPSProvider : AbstractShippingProvider
{
    public override string Name => "UPS";

    readonly UPSProviderConfiguration _configuration;
    readonly ILogger<UPSProvider>? _logger;

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
        SetHttpClient(httpClient);
    }

    public UPSProvider(UPSProviderConfiguration configuration, ILogger<UPSProvider> logger)
        : this(configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public UPSProvider(UPSProviderConfiguration configuration, HttpClient httpClient, ILogger<UPSProvider> logger)
        : this(configuration, httpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<RateResult> GetRatesAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        using var httpClientLease = RentHttpClient();
        var httpClient = httpClientLease.HttpClient;
        var resultBuilder = new RateResultAggregator(Name);

        try
        {
            var oauthService = new UpsOAuthClient(_logger);
            var token = await oauthService.GetTokenAsync(_configuration, httpClient, resultBuilder, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(token))
            {
                var requestBuilder = new UpsRatingRequestBuilder(_configuration);
                var request = requestBuilder.Build(shipment);

                var ratingService = new UpsRatingService(_logger);
                var ratingsResponse = await ratingService.GetRatingAsync(httpClient, token, _configuration.UseProduction, request, resultBuilder, cancellationToken).ConfigureAwait(false);

                ParseResponse(shipment, ratingsResponse, resultBuilder);
            }
        }
        catch (Exception e)
        {
            resultBuilder.AddInternalError($"UPS Provider Exception: {e.Message}");
            _logger?.LogError(e, "UPS Provider Exception");
        }

        return resultBuilder.Build();
    }

    private void ParseResponse(Shipment shipment, UpsRatingResponse? response, RateResultAggregator resultBuilder)
    {
        if (response?.RateResponse?.RatedShipment == null)
            return;

        var cultureInfo = new CultureInfo("en-US");     // Response is always in en-US

        foreach (var rate in response.RateResponse.RatedShipment)
        {
            if (rate == null) continue;

            var serviceCode = rate.Service?.Code;
            if (serviceCode == null || !_serviceCodes.ContainsKey(serviceCode))
            {
                resultBuilder.AddInternalError($"Unknown service code {serviceCode}");
                continue;
            }
            var serviceDescription = _serviceCodes[serviceCode];

            if (!decimal.TryParse(rate.TotalCharges?.MonetaryValue, NumberStyles.Number, cultureInfo, out var totalCharges))
            {
                resultBuilder.AddInternalError($"Invalid total charges value for service code {serviceCode}");
                continue;
            }

            var currencyCode = rate.TotalCharges?.CurrencyCode;

            if (_configuration.UseNegotiatedRates && rate.NegotiatedRateCharges != null)
            {
                if (decimal.TryParse(rate.NegotiatedRateCharges.TotalCharge?.MonetaryValue, NumberStyles.Number, cultureInfo, out var negotiatedTotalCharges))
                {
                    totalCharges = negotiatedTotalCharges;
                }
                else
                {
                    resultBuilder.AddInternalError($"Invalid negotiated total charges value for service code {serviceCode}");
                    continue;
                }

                currencyCode = rate.NegotiatedRateCharges.TotalCharge?.CurrencyCode;
            }

            // Use MaxDate as default to ensure correct sorting
            var estDeliveryDate = DateTime.MaxValue.ToShortDateString();
            var businessDaysInTransit = rate.GuaranteedDelivery?.BusinessDaysInTransit;
            if (!string.IsNullOrEmpty(businessDaysInTransit))
            {
                if (double.TryParse(businessDaysInTransit, NumberStyles.Number, cultureInfo, out var businessDays))
                {
                    estDeliveryDate = (shipment.Options.ShippingDate ?? DateTime.Now)
                        .AddDays(businessDays).ToShortDateString();
                }
                else
                {
                    resultBuilder.AddInternalError($"Invalid BusinessDaysInTransit value for service code {serviceCode}");
                }
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

            if (!DateTime.TryParse(estDeliveryDate, cultureInfo, DateTimeStyles.None, out var deliveryDate))
            {
                resultBuilder.AddInternalError($"Invalid delivery date value for service code {serviceCode}");
                deliveryDate = DateTime.MaxValue;
            }

            resultBuilder.AddRate(serviceCode, serviceDescription, totalCharges, deliveryDate, new RateOptions()
            {
                SaturdayDelivery = shipment.Options.SaturdayDelivery && deliveryDate.DayOfWeek == DayOfWeek.Saturday
            }, currencyCode ?? ShipmentOptions.DefaultCurrencyCode);
        }
    }

    public static IDictionary<string, string> GetServiceCodes() => _serviceCodes;
}
