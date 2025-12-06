using Microsoft.Extensions.Logging;
using ShippingRates.Models;
using ShippingRates.Models.Usps;
using ShippingRates.Services.Usps;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ShippingRates.ShippingProviders.Usps;

public class UspsProvider : AbstractShippingProvider
{
    public override string Name { get => "USPS"; }

    const string UspsCurrencyCode = "USD";

    readonly UspsProviderConfiguration _configuration;
    readonly ILogger<UspsProvider>? _logger;

    public UspsProvider(UspsProviderConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        if (string.IsNullOrEmpty(_configuration.ClientId))
            throw new Exception("ClientId is required");
        if (string.IsNullOrEmpty(_configuration.ClientSecret))
            throw new Exception("ClientSecret is required");
    }

    public UspsProvider(UspsProviderConfiguration configuration, HttpClient httpClient)
        : this(configuration)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public UspsProvider(UspsProviderConfiguration configuration, ILogger<UspsProvider> logger)
        : this(configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public UspsProvider(UspsProviderConfiguration configuration, HttpClient httpClient, ILogger<UspsProvider> logger)
        : this(configuration, httpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<RateResult> GetRatesAsync(Shipment shipment)
    {
        var resultBuilder = new RateResultAggregator(Name);

        if (!shipment.OriginAddress.IsUnitedStatesAddress())
        {
            resultBuilder.AddInternalError(UspsMessages.Error.ErrorOriginNotUS);
            _logger?.LogError(UspsMessages.Error.ErrorOriginNotUS);
            return resultBuilder.Build();
        }

        var httpClient = IsExternalHttpClient ? HttpClient : new HttpClient();

        try
        {
            var oauthService = new UspsOAuthClient(_logger);
            var token = await oauthService.GetTokenAsync(_configuration, httpClient, resultBuilder);

            if (token is { Length: > 0 })
            {
                if (shipment.DestinationAddress.IsUnitedStatesAddress())
                {
                    var ratingService = new UspsPricesService(_logger);
                    var domesticRequest = GetDomesticRequest(shipment);
                    var pricesResponse = await ratingService.GetDomesticPrices(
                        httpClient, token, domesticRequest, _configuration.UseProduction, resultBuilder);

                    ParsePricesResponse(shipment, pricesResponse, resultBuilder);

                    //var ratingService = new UspsShippingOptionsService(_logger);
                    //var request = GetRequest(shipment);
                    //var optionsResponse = await ratingService.GetShippingOptions(
                    //    httpClient, token, request, _configuration.UseProduction, resultBuilder);

                    //ParseOptionsResponse(shipment, optionsResponse, resultBuilder);
                }
                else
                {
                    var ratingService = new UspsPricesService(_logger);
                    var internationalRequest = GetInternationalRequest(shipment);
                    var pricesResponse = await ratingService.GetInternationalPrices(
                        httpClient, token, internationalRequest, _configuration.UseProduction, resultBuilder);

                    ParsePricesResponse(shipment, pricesResponse, resultBuilder);
                }
            }
        }
        catch (Exception e)
        {
            resultBuilder.AddInternalError($"USPS Provider Exception: {e.Message}");
            _logger?.LogError(e, "USPS Provider Exception");
        }
        finally
        {
            if (!IsExternalHttpClient && httpClient != null)
                httpClient.Dispose();
        }

        return resultBuilder.Build();
    }

    private void ParseOptionsResponse(Shipment shipment, UspsShippingOptionsResponse? response, RateResultAggregator resultAggregator)
    {
        if (response?.PricingOptions == null)
            return;

        var cultureInfo = new CultureInfo("en-US");     // Response is always in en-US
        var resultRates = new List<UnfilteredRate>();

        foreach (var pricingOption in response.PricingOptions)
        {
            if (pricingOption.Options == null) continue;

            foreach (var option in pricingOption.Options)
            {
                if (option.RateOptions == null) continue;

                foreach (var rateOption in option.RateOptions)
                {
                    if (rateOption.Rates == null) continue;

                    var rates = rateOption.Rates.Where(r => r.ProcessingCategory == _configuration.ProcessingCategory).ToList();

                    if (rates.Count == 0) continue;

                    var rate = rateOption.Rates[0];
                    var totalCharges = Convert.ToDecimal(rateOption.TotalPrice, cultureInfo);
                    var deliveryDate = rateOption.Commitment?.ScheduleDeliveryDate ?? DateTime.MaxValue;
                    var rateName = !string.IsNullOrEmpty(rate.ProductName)
                        ? rate.ProductName
                        : rate.Description;
                    rateName ??= string.Empty;

                    // Skip DDU/DSCF/SCF
                    if (rateName.Contains("DDU") || rateName.Contains("DSCF") || rateName.Contains("SCF")) continue;

                    resultRates.Add(new UnfilteredRate()
                    {
                        ProviderCode = rateName,
                        Name = rateName,
                        TotalCharges = totalCharges,
                        DeliveryDate = deliveryDate
                    });
                }
            }
        }

        var filteredRates = resultRates
            .GroupBy(r => r.ProviderCode)
            .Select(g =>
            {
                var min = g.Min(r => r.TotalCharges);
                return g.First(r => r.TotalCharges == min);
            })
            .ToList();

        foreach (var rate in filteredRates.OrderBy(r => r.TotalCharges).ThenBy(r => r.DeliveryDate))
        {
            resultAggregator.AddRate(rate.ProviderCode, rate.Name, rate.TotalCharges, rate.DeliveryDate, new RateOptions()
            {
                SaturdayDelivery = shipment.Options.SaturdayDelivery && rate.DeliveryDate.DayOfWeek == DayOfWeek.Saturday
            }, UspsCurrencyCode);
        }
    }

    private void ParsePricesResponse(Shipment shipment, UspsPricesResponse? response, RateResultAggregator resultAggregator)
    {
        if (response?.RateOptions == null)
            return;

        var cultureInfo = new CultureInfo("en-US");     // Response is always in en-US
        var resultRates = new List<UnfilteredRate>();

        foreach (var rateOption in response.RateOptions)
        {
            if (rateOption.Rates == null) continue;

            var rates = rateOption.Rates.Where(r => r.ProcessingCategory == _configuration.ProcessingCategory).ToList();

            if (rates.Count == 0) continue;

            var rate = rates[0];
            var totalCharges = Convert.ToDecimal((rateOption.TotalPrice ?? 0) != 0 ? rateOption.TotalPrice: rateOption.TotalBasePrice, cultureInfo);
            var deliveryDate = DateTime.MaxValue;       // TODO: Need fetch data from ShippingOptions API
            var rateName = !string.IsNullOrEmpty(rate.ProductName)
                ? rate.ProductName
                : rate.Description;

            rateName ??= string.Empty;

            // Skip DDU/DSCF/SCF
            if (rateName.Contains("DDU") || rateName.Contains("DSCF") || rateName.Contains("SCF")) continue;

            resultRates.Add(new UnfilteredRate()
            {
                ProviderCode = rateName,
                Name = rateName,
                TotalCharges = totalCharges,
                DeliveryDate = deliveryDate
            });
        }

        var filteredRates = resultRates
            .GroupBy(r => r.ProviderCode)
            .Select(g =>
            {
                var min = g.Min(r => r.TotalCharges);
                return g.First(r => r.TotalCharges == min);
            })
            .ToList();

        foreach (var rate in filteredRates.OrderBy(r => r.TotalCharges).ThenBy(r => r.DeliveryDate))
        {
            resultAggregator.AddRate(rate.ProviderCode, rate.Name, rate.TotalCharges, rate.DeliveryDate, new RateOptions()
            {
                SaturdayDelivery = shipment.Options.SaturdayDelivery && rate.DeliveryDate.DayOfWeek == DayOfWeek.Saturday
            }, UspsCurrencyCode);
        }
    }

    private void SetBaseRequest(UspsRequestBase request, Shipment shipment)
    {
        var extraServices = new List<UspsExtraServiceCode>();

        request.OriginZipCode = shipment.OriginAddress.PostalCode;
        request.Weight = (double)shipment.GetTotalPackageWeight();
        request.MailingDate = shipment.Options.ShippingDate;
        request.Length = (double)shipment.Packages.Max(p => p.Length);
        request.Height = (double)shipment.Packages.Max(p => p.Height);
        request.Width = (double)shipment.Packages.Max(p => p.Width);
        request.PriceType = _configuration.PriceType;
        request.AccountType = _configuration.AccountType;
        request.AccountNumber = _configuration.AccountNumber;

        var insuranceValue = shipment.Packages.Sum(p => p.InsuredValue);
        request.ItemValue = (double)insuranceValue;

        if (insuranceValue > 500)
        {
            extraServices.Add(UspsExtraServiceCode.InsuranceOver500);
        }
        else if (insuranceValue > 0)
        {
            extraServices.Add(UspsExtraServiceCode.InsuranceUpTo500);
        }

        if (shipment.Packages.Any(p => p.SignatureRequiredOnDelivery))
        {
            extraServices.Add(UspsExtraServiceCode.AdultSignatureRequired);
            extraServices.Add(UspsExtraServiceCode.CertifiedMailAdultSignatureRequired);
        }

        extraServices.AddRange(_configuration.ExtraServiceCodes);
        request.ExtraServices = [.. extraServices.Distinct()];
    }

    private UspsInternationalRequest GetInternationalRequest(Shipment shipment)
    {
        var request = new UspsInternationalRequest()
        {
            ForeignPostalCode = shipment.DestinationAddress.PostalCode,
            DestinationCountryCode = shipment.DestinationAddress.CountryCode,
            MailClass = _configuration.MailClasses.Length > 0
                ? _configuration.MailClasses[0]
                : UspsMailClass.All
        };

        SetBaseRequest(request, shipment);

        return request;
    }

    private UspsDomesticRequest GetDomesticRequest(Shipment shipment)
    {
        var request = new UspsDomesticRequest()
        {
            DestinationZipCode = shipment.DestinationAddress.PostalCode,
            MailClasses = _configuration.MailClasses.Length > 0
                ? _configuration.MailClasses
                : [UspsMailClass.All]
        };

        SetBaseRequest(request, shipment);

        return request;
    }

    private UspsShippingOptionsRequest GetRequest(Shipment shipment)
    {
        var pricingOption = new UpspPricingOptions()
        {
            PriceType = _configuration.PriceType
        };

        if (!string.IsNullOrEmpty(_configuration.AccountType) || !string.IsNullOrEmpty(_configuration.AccountNumber))
        {
            pricingOption.PaymentAccount = new UspsPaymentAccount()
            {
                AccountType = _configuration.AccountType,
                AccountNumber = _configuration.AccountNumber
            };
        }

        var request = new UspsShippingOptionsRequest()
        {
            OriginZipCode = shipment.OriginAddress.PostalCode,
            DestinationZipCode = shipment.DestinationAddress.PostalCode,
            PricingOptions = [pricingOption],
            PackageDescription = new UspsDomesticPackageDescription()
            {
                Weight = (double)shipment.GetTotalPackageWeight(),
                Length = (double)shipment.Packages.Max(p => p.Length),
                Height = (double)shipment.Packages.Max(p => p.Height),
                Width = (double)shipment.Packages.Max(p => p.Width),
                MailingDate = shipment.Options.ShippingDate,
                MailClass = _configuration.MailClasses.Length > 0
                    ? _configuration.MailClasses[0]
                    : UspsMailClass.All,
                ItemValue = (double)shipment.Packages.Sum(p => p.InsuredValue),
            },
            //ShippingFilter = "PRICE"
        };

        var extraServices = new List<UspsExtraServiceCode>();

        if (shipment.Packages.Any(p => p.SignatureRequiredOnDelivery))
        {
            extraServices.Add(UspsExtraServiceCode.AdultSignatureRequired);
            extraServices.Add(UspsExtraServiceCode.CertifiedMailAdultSignatureRequired);
        }

        // TODO: Fix
        //if (shipment.Packages.Any(p => p.InsuredValue > 0))
        //{
        //    extraServices.Add(UspsExtraServiceCode.InsuranceUpTo500);
        //    extraServices.Add(UspsExtraServiceCode.InsuranceOver500);
        //    extraServices.Add(UspsExtraServiceCode.PriorityMailExpressMerchandiseInsurance);
        //}

        extraServices.AddRange(_configuration.ExtraServiceCodes);
        request.PackageDescription.ExtraServices = [.. extraServices.Distinct()];

        return request;
    }

    class UnfilteredRate
    {
        public string ProviderCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal TotalCharges { get; set; }
        public DateTime DeliveryDate { get; set; }
    }
}
