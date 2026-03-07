using Microsoft.Extensions.Logging;
using ShippingRates.Helpers.Extensions;
using ShippingRates.Models;
using ShippingRates.OpenApi.FedEx.FrieghtLtl;
using ShippingRates.Services.FedEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ShippingRates.ShippingProviders.FedEx;

/// <summary>
///     Provides Freight LTL rates (only) from FedEx (Federal Express) REST API.
/// </summary>
public class FedExFreightLtlProvider : FedExBaseProvider<FedExFreightLtlProvider>
{
    public override string Name { get => "FedExFreightLtl"; }

    public FedExFreightLtlProvider(FedExProviderConfiguration configuration)
        : base(configuration) { }

    public FedExFreightLtlProvider(FedExProviderConfiguration configuration, HttpClient httpClient)
        : base(configuration, httpClient) { }

    public FedExFreightLtlProvider(FedExProviderConfiguration configuration, ILogger<FedExFreightLtlProvider> logger)
        : base(configuration, logger) { }

    public FedExFreightLtlProvider(FedExProviderConfiguration configuration, HttpClient httpClient, ILogger<FedExFreightLtlProvider> logger)
        : base(configuration, httpClient, logger) { }

    /// <summary>
    /// Sets the service codes.
    /// </summary>
    protected override Dictionary<string, string> ServiceCodes => new()
    {
        { "FEDEX_FREIGHT_PRIORITY", "FedEx Freight Priority" },
        { "FEDEX_FREIGHT_ECONOMY", "FedEx Freight Economy" }
    };

    /// <summary>
    /// Creates the rate request
    /// </summary>
    /// <returns></returns>
    protected FullSchema CreateRateRequest(Shipment shipment)
    {
        // Build the RateRequest
        var request = new FullSchema
        {
            AccountNumber = new LTLRootAccountNumber() { Value = _configuration.AccountNumber },
            FreightRequestedShipment = new LTLRequestedShipment
            {
                Shipper = new RateParty
                {
                    Address = new RateAddress
                    {
                        PostalCode = shipment.OriginAddress.PostalCode,
                        CountryCode = shipment.OriginAddress.CountryCode
                    }
                },
                Recipient = new RateParty
                {
                    Address = new RateAddress
                    {
                        PostalCode = shipment.DestinationAddress.PostalCode,
                        CountryCode = shipment.DestinationAddress.CountryCode
                    }
                },
                FreightShipmentDetail = new LTLRequestedShipmentFreightShipmentDetail
                {
                    Role = LTLRequestedShipmentFreightShipmentDetailRole.SHIPPER,
                    AccountNumber = new AccountNumber() { Value = _configuration.AccountNumber },
                },
                RateRequestType = [.. FedExFreightLtlProvider.GetRateRequestTypes()],
                PreferredCurrency = shipment.Options.GetCurrencyCode(),
                TotalPackageCount = shipment.Packages.Count,
            },
        };

        var shipDateStamp = shipment.Options.ShippingDate == null ? null : DateTime.Now.ToString("yyyy-MM-dd"); // Shipping date and time
        if (shipDateStamp != null)
        {
            request.FreightRequestedShipment.ShipDateStamp = shipDateStamp;
        }

        if (shipment.Options.SaturdayDelivery)
        {
            request.RateRequestControlParameters = new RateRequestControlParameters
            {
                VariableOptions = RateRequestControlParametersVariableOptions.SATURDAY_DELIVERY
            };
        }

        SetPackageLineItems(request, shipment);

        return request;
    }
    private static IEnumerable<RateRequestType> GetRateRequestTypes()
    {
        yield return RateRequestType.LIST;
        yield return RateRequestType.ACCOUNT;
    }

    /// <summary>
    /// Sets package line items
    /// </summary>
    /// <param name="request"></param>
    protected void SetPackageLineItems(FullSchema request, Shipment shipment)
    {
        request.FreightRequestedShipment.RequestedPackageLineItems = [];

        var i = 0;
        foreach (var package in shipment.Packages)
        {
            var item = new LTLRequestedPackageLineItem()
            {
                SubPackagingType = shipment.Packages[i].Container ?? "CONTAINER",
                // Package weight
                Weight = new Weight_With_Link()
                {
                    Units = Weight_With_LinkUnits.LB,
                    Value = (double)shipment.Packages[i].GetRoundedWeight(UnitsSystem.USCustomary),
                },

                // Package dimensions
                Dimensions = new RequestePackageLineItemDimensions()
                {
                    Length = (int)shipment.Packages[i].GetRoundedLength(UnitsSystem.USCustomary),
                    Width = (int)shipment.Packages[i].GetRoundedWidth(UnitsSystem.USCustomary),
                    Height = (int)shipment.Packages[i].GetRoundedHeight(UnitsSystem.USCustomary),
                    Units = "IN",
                }
            };

            if (_allowInsuredValues && package.InsuredValue > 0)
            {
                // package insured value
                item.DeclaredValue = new Money
                {
                    Amount = (double)package.InsuredValue,
                    Currency = "USD"
                };
            }

            request.FreightRequestedShipment.RequestedPackageLineItems.Add(item);

            i++;
        }
    }

    /// <summary>
    /// Gets rates
    /// </summary>
    public override async Task<RateResult> GetRatesAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        var resultBuilder = new RateResultAggregator(Name);

        using var httpClientLease = RentHttpClient();
        var httpClient = httpClientLease.HttpClient;

        try
        {
            var oauthService = new FedExOAuthClient(_logger);
            var token = await oauthService.GetTokenAsync(_configuration, httpClient, resultBuilder, cancellationToken).ConfigureAwait(false);

            if (token is { Length: > 0 })
            {
                var request = CreateRateRequest(shipment);

                var service = new Client(httpClient)
                {
                    BaseUrl = GetRequestUri(_configuration.UseProduction)
                };
                var reply = await service.Freight_RateQuoteAsync(
                    request,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    "application/json",
                    "en-US",
                    "Bearer " + token,
                    cancellationToken
                ).ConfigureAwait(false);

                if (reply.Output != null)
                {
                    ProcessReply(resultBuilder, shipment, reply.Output);
                }
                else
                {
                    resultBuilder.AddInternalError($"FedEx Freight LTL provider: API returned NULL result");
                }
            }
        }
        catch (ApiException e)
        {
            FedExFreightLtlProvider.ProcessErrors(resultBuilder, e);
        }
        catch (Exception e)
        {
            resultBuilder.AddInternalError($"FedEx Freight LTL Provider Exception: {e.Message}");
            _logger?.LogError(e, "FedEx Freight LTL Provider Exception");
        }

        return resultBuilder.Build();
    }

    /// <summary>
    /// Processes the reply
    /// </summary>
    /// <param name="reply"></param>
    internal void ProcessReply(RateResultAggregator rateResult, Shipment shipment, BaseProcessOutputVO_Rate reply)
    {
        if (reply?.RateReplyDetails == null)
            return;

        foreach (var rateReplyDetail in reply.RateReplyDetails)
        {
            var key = rateReplyDetail.ServiceType.ToString();

            if (!ServiceCodes.TryGetValue(key, out var serviceCode))
            {
                rateResult.AddInternalError($"Unknown FedEx Freight LTL rate code: {key}");
            }
            else
            {
                var rateDetails = FedExFreightLtlProvider.GetRateDetailsByRateType(rateReplyDetail);
                if (rateDetails == null || rateDetails.Length == 0)
                {
                    continue;
                }

                var rates = rateDetails.Select(r => FedExFreightLtlProvider.GetCurrencyConvertedRate(shipment, r));
                rates = rates.Any(r => r.currencyCode == shipment.Options.GetCurrencyCode())
                    ? rates.Where(r => r.currencyCode == shipment.Options.GetCurrencyCode())
                    : rates;

                var (amount, currencyCode) = rates.OrderByDescending(r => r.amount).FirstOrDefault();

                DateTime? deliveryDate = null;
                if (rateReplyDetail.OperationalDetail.DeliveryDate != null)
                {
                    if (DateTime.TryParse(rateReplyDetail.OperationalDetail.DeliveryDate, out DateTime parsedDate))
                    {
                        deliveryDate = parsedDate;
                    }
                }

                if (deliveryDate == null)
                {
                    deliveryDate = DateTime.Now.AddDays(30);
                }


                rateResult.AddRate(key, serviceCode, amount, deliveryDate.Value, new RateOptions()
                {
                    SaturdayDelivery = rateReplyDetail.Commit?.SaturdayDelivery ?? false
                },
                currencyCode);
            }
        }
    }

    private static RatedShipmentDetail[] GetRateDetailsByRateType(LTLRateReplyDetail rateReplyDetail)
    {
        var negotiatedRateTypes = new RatedShipmentDetailRateType[]
        {
            RatedShipmentDetailRateType.ACCOUNT,
            RatedShipmentDetailRateType.CUSTOM,
            RatedShipmentDetailRateType.INCENTIVE,
            RatedShipmentDetailRateType.ACTUAL
        };

        return negotiatedRateTypes.Contains(rateReplyDetail.RatedShipmentDetails.RateType) ?
            [rateReplyDetail.RatedShipmentDetails] :
            [];
    }

    private static (decimal amount, string currencyCode) GetCurrencyConvertedRate(Shipment shipment, RatedShipmentDetail rateDetail)
    {
        var shipmentCurrencyCode = shipment.Options.GetCurrencyCode();

        if (rateDetail?.TotalNetCharge == null)
            return (0, shipmentCurrencyCode);

        var needCurrencyConversion = rateDetail.ShipmentRateDetail.Currency != shipmentCurrencyCode;
        if (!needCurrencyConversion)
            return ((decimal)rateDetail.TotalNetCharge, shipmentCurrencyCode);

        var canConvertCurrency = rateDetail.ShipmentRateDetail.CurrencyExchangeRate != null
            && rateDetail.ShipmentRateDetail.Currency == rateDetail.ShipmentRateDetail.CurrencyExchangeRate.IntoCurrency
            && shipmentCurrencyCode == rateDetail.ShipmentRateDetail.CurrencyExchangeRate.FromCurrency
            && rateDetail.ShipmentRateDetail.CurrencyExchangeRate.Rate != 1
            && rateDetail.ShipmentRateDetail.CurrencyExchangeRate.Rate != 0;

        if (!canConvertCurrency)
            return ((decimal)rateDetail.TotalNetCharge, rateDetail.ShipmentRateDetail.Currency);

        return ((decimal)Math.Round(rateDetail.TotalNetCharge / rateDetail.ShipmentRateDetail.CurrencyExchangeRate.Rate, 2), shipmentCurrencyCode);
    }

    private static void ProcessErrors(RateResultAggregator rateResult, ApiException exception)
    {
        var msg = exception.Message;
        if (exception is ApiException<ErrorResponseVO> typedResponseException)
        {
            if (typedResponseException.Result?.Errors != null)
            {
                msg = string.Join(", ", typedResponseException.Result.Errors.Select(e => e.Code));
            }
        }

        var err = new Error
        {
            Description = msg,
            Source = exception.Source,
            Number = exception.StatusCode.ToString()
        };

        rateResult.AddProviderError(err);
    }
}
