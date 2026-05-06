using Microsoft.Extensions.Logging;
using ShippingRates.Helpers.Extensions;
using ShippingRates.Models;
using ShippingRates.OpenApi.FedEx.RateTransitTimes;
using ShippingRates.Services.FedEx;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ShippingRates.ShippingProviders.FedEx
{
    public abstract class FedExRateTransmitTimesBaseProvider<T> : FedExBaseProvider<T> where T: FedExRateTransmitTimesBaseProvider<T>
    {
        public FedExRateTransmitTimesBaseProvider(FedExProviderConfiguration configuration) : base(configuration)
        {
        }

        public FedExRateTransmitTimesBaseProvider(FedExProviderConfiguration configuration, HttpClient httpClient)
            : base(configuration, httpClient)
        {
        }

        public FedExRateTransmitTimesBaseProvider(FedExProviderConfiguration configuration, ILogger<T> logger)
            : base(configuration, logger)
        {
        }

        public FedExRateTransmitTimesBaseProvider(FedExProviderConfiguration configuration, HttpClient httpClient, ILogger<T> logger)
            : base(configuration, httpClient, logger)
        {
        }

        /// <summary>
        /// Creates the rate request
        /// </summary>
        /// <returns></returns>
        protected Full_Schema_Quote_Rate CreateRateRequest(Shipment shipment)
        {
            // Build the RateRequest
            var request = new Full_Schema_Quote_Rate
            {
                AccountNumber = new AccountNumber() { Value = _configuration.AccountNumber },
                RequestedShipment = new RequestedShipment
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
                    PickupType = ToApiPickupType(_configuration.PickupType),
                    RateRequestType = [.. GetRateRequestTypes(shipment)],
                    PreferredCurrency = shipment.Options.GetCurrencyCode(),
                    PackagingType = ToApiPackagingType(
                        shipment.Options.FedExPackagingTypeOverride ?? _configuration.PackagingType),
                    TotalPackageCount = shipment.Packages.Count,
                },
            };

            if (shipment.Options.ShippingDate != null)
            {
                request.RequestedShipment.ShipDateStamp = shipment.Options.ShippingDate.Value.ToString("yyyy-MM-dd");
            }

            if (shipment.Options.FedExOneRate)
            {
                request.RequestedShipment.PackagingType = GetPackagingTypeForOneRate(shipment);
                request.RequestedShipment.ShipmentSpecialServices = new RequestedShipmentSpecialServicesRequested()
                {
                    SpecialServiceTypes = ["FEDEX_ONE_RATE"]
                };
            }

            request.RateRequestControlParameters = new RateRequestControlParameters
            {
                ReturnTransitTimes = true
            };

            if (shipment.Options.SaturdayDelivery)
            {
                request.RateRequestControlParameters.VariableOptions = RateRequestControlParametersVariableOptions.SATURDAY_DELIVERY;
                request.RateRequestControlParameters.SerializeVariableOptions = true;
            }

            SetPackageLineItems(request, shipment);
            SetShipmentDetails(request);

            return request;
        }

        private static IEnumerable<RateRequestType> GetRateRequestTypes(Shipment shipment)
        {
            yield return RateRequestType.LIST;
            yield return RateRequestType.ACCOUNT;
            if (!string.IsNullOrEmpty(shipment.Options.PreferredCurrencyCode))
            {
                yield return RateRequestType.PREFERRED;
            }
        }

        private static RequestedShipmentPickupType ToApiPickupType(FedExPickupType pickupType)
            => pickupType switch
            {
                FedExPickupType.ContactFedExToSchedule => RequestedShipmentPickupType.CONTACT_FEDEX_TO_SCHEDULE,
                FedExPickupType.DropoffAtFedExLocation => RequestedShipmentPickupType.DROPOFF_AT_FEDEX_LOCATION,
                _ => RequestedShipmentPickupType.USE_SCHEDULED_PICKUP
            };

        private string GetPackagingTypeForOneRate(Shipment shipment)
        {
            if (shipment.Options.FedExPackagingTypeOverride.HasValue)
            {
                return ToApiPackagingType(shipment.Options.FedExPackagingTypeOverride.Value);
            }

#pragma warning disable CS0618 // Backward compatibility fallback for deprecated API.
            if (!string.IsNullOrEmpty(shipment.Options.FedExOneRatePackageOverride))
            {
                return shipment.Options.FedExOneRatePackageOverride;
            }
#pragma warning restore CS0618

            return "FEDEX_MEDIUM_BOX";
        }

        private static string ToApiPackagingType(FedExPackagingType packagingType)
            => packagingType switch
            {
                FedExPackagingType.FedExEnvelope => "FEDEX_ENVELOPE",
                FedExPackagingType.FedExPak => "FEDEX_PAK",
                FedExPackagingType.FedExBox => "FEDEX_BOX",
                FedExPackagingType.FedExTube => "FEDEX_TUBE",
                FedExPackagingType.FedEx10KgBox => "FEDEX_10KG_BOX",
                FedExPackagingType.FedEx25KgBox => "FEDEX_25KG_BOX",
                FedExPackagingType.FedExSmallBox => "FEDEX_SMALL_BOX",
                FedExPackagingType.FedExMediumBox => "FEDEX_MEDIUM_BOX",
                FedExPackagingType.FedExLargeBox => "FEDEX_LARGE_BOX",
                FedExPackagingType.FedExExtraLargeBox => "FEDEX_EXTRA_LARGE_BOX",
                _ => "YOUR_PACKAGING"
            };

        /// <summary>
        /// Sets shipment details
        /// </summary>
        /// <param name="request"></param>
        protected abstract void SetShipmentDetails(Full_Schema_Quote_Rate request);

        /// <summary>
        /// Sets package line items
        /// </summary>
        /// <param name="request"></param>
        protected void SetPackageLineItems(Full_Schema_Quote_Rate request, Shipment shipment)
        {
            request.RequestedShipment.RequestedPackageLineItems = [];

            var i = 0;
            foreach (var package in shipment.Packages)
            {
                var item = new RequestedPackageLineItem()
                {
                    // Package weight
                    Weight = new Weight_2()
                    {
                        Units = "LB",
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

                if (package.SignatureRequiredOnDelivery)
                {
                    item.PackageSpecialServices = new PackageSpecialServicesRequested()
                    {
                        SignatureOptionType = PackageSpecialServicesRequestedSignatureOptionType.DIRECT
                    };
                }

                request.RequestedShipment.RequestedPackageLineItems.Add(item);

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
                    var reply = await service.Rate_and_Transit_timesAsync(
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
                        resultBuilder.AddInternalError($"FedEx provider: API returned NULL result");
                    }
                }
            }
            catch (ApiException e)
            {
                ProcessErrors(resultBuilder, e);
            }
            catch (Exception e)
            {
                resultBuilder.AddInternalError($"FedEx Provider Exception: {e.Message}");
                _logger?.LogError(e, "FedEx Provider Exception");
            }

            return resultBuilder.Build();
        }

        /// <summary>
        /// Processes the reply
        /// </summary>
        /// <param name="reply"></param>
        internal void ProcessReply(RateResultAggregator rateResult, Shipment shipment, BaseProcessOutputVO reply)
        {
            if (reply?.RateReplyDetails == null)
                return;

            foreach (var rateReplyDetail in reply.RateReplyDetails)
            {
                var key = rateReplyDetail.ServiceType;

                if (!ServiceCodes.TryGetValue(key, out var serviceCode))
                {
                    rateResult.AddInternalError($"Unknown FedEx rate code: {key}");
                }
                else
                {
                    var rateDetails = GetRateDetailsByRateType(rateReplyDetail);
                    if (rateDetails == null || rateDetails.Length == 0)
                    {
                        // For non-US to non-US shipments without preferred non-US currency, we can have receive negotiated rates
                        if (!shipment.OriginAddress.IsUnitedStatesAddress() && !shipment.DestinationAddress.IsUnitedStatesAddress())
                        {
                            rateDetails = [.. rateReplyDetail.RatedShipmentDetails];
                        }
                    }
                    if (rateDetails == null || rateDetails.Length == 0)
                    {
                        continue;
                    }

                    var rates = rateDetails.Select(r => GetCurrencyConvertedRate(shipment, r));
                    rates = rates.Any(r => r.currencyCode == shipment.Options.GetCurrencyCode())
                        ? rates.Where(r => r.currencyCode == shipment.Options.GetCurrencyCode())
                        : rates;

                    var (amount, currencyCode) = rates.OrderByDescending(r => r.amount).FirstOrDefault();

                    var deliveryDate = GetDeliveryDate(rateReplyDetail) ?? DateTime.Now.AddDays(30);

                    rateResult.AddRate(key, serviceCode, amount, deliveryDate, new RateOptions()
                    {
                        SaturdayDelivery = rateReplyDetail.Commit?.SaturdayDelivery ?? false
                    },
                    currencyCode);
                }
            }
        }

        private static DateTime? GetDeliveryDate(RateReplyDetail rateReplyDetail)
        {
            var deliveryDate = ParseFedExDate(rateReplyDetail.OperationalDetail?.DeliveryDate);
            if (deliveryDate != null)
            {
                return deliveryDate;
            }

            deliveryDate = ParseFedExDate(rateReplyDetail.OperationalDetail?.CommitDate);
            if (deliveryDate != null)
            {
                return deliveryDate;
            }

            return ParseFedExDate(rateReplyDetail.Commit?.DateDetail?.DayFormat);
        }

        private static DateTime? ParseFedExDate(string? value)
        {
            return DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate)
                ? parsedDate
                : null;
        }

        private RatedShipmentDetail[] GetRateDetailsByRateType(RateReplyDetail rateReplyDetail)
        {
            var negotiatedRateTypes = new RatedShipmentDetailRateType[]
            {
                RatedShipmentDetailRateType.ACCOUNT,
                RatedShipmentDetailRateType.CUSTOM,
                RatedShipmentDetailRateType.ACTUAL
            };

            return rateReplyDetail.RatedShipmentDetails
                .Where(rsd =>
                    _configuration.UseNegotiatedRates && negotiatedRateTypes.Contains(rsd.RateType) ||
                    !_configuration.UseNegotiatedRates && !negotiatedRateTypes.Contains(rsd.RateType))
                .ToArray();
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

        private void ProcessErrors(RateResultAggregator rateResult, ApiException exception)
        {
            var msg = GetFedExErrorMessage(exception) ?? exception.Message;

            var err = new Error
            {
                Description = msg,
                Source = exception.Source,
                Number = exception.StatusCode.ToString()
            };

            rateResult.AddProviderError(err);
        }

        private static string? GetFedExErrorMessage(ApiException exception)
            => exception switch
            {
                ApiException<ErrorResponseVO> typedResponseException => JoinMessages(
                    typedResponseException.Result?.Errors?.Select(e => ((string?)e.Code, (string?)e.Message))),
                ApiException<ErrorResponseVO401> typedResponseException401 => JoinMessages(
                    typedResponseException401.Result?.Errors?.Select(e => ((string?)e.Code, Convert.ToString(e.Message)))),
                ApiException<ErrorResponseVO403> typedResponseException403 => JoinMessages(
                    typedResponseException403.Result?.Errors?.Select(e => ((string?)e.Code, Convert.ToString(e.Message)))),
                ApiException<ErrorResponseVO404> typedResponseException404 => JoinMessages(
                    typedResponseException404.Result?.Errors?.Select(e => ((string?)e.Code, Convert.ToString(e.Message)))),
                ApiException<ErrorResponseVO500> typedResponseException500 => JoinMessages(
                    typedResponseException500.Result?.Errors?.Select(e => ((string?)e.Code, Convert.ToString(e.Message)))),
                ApiException<ErrorResponseVO503> typedResponseException503 => JoinMessages(
                    typedResponseException503.Result?.Errors?.Select(e => ((string?)e.Code, Convert.ToString(e.Message)))),
                _ => null
            };

        private static string? JoinMessages(IEnumerable<(string? code, string? message)>? errors)
        {
            if (errors == null)
            {
                return null;
            }

            var messages = errors
                .Select(error => FormatMessage(error.code, error.message))
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .ToArray();

            return messages.Length == 0 ? null : string.Join(", ", messages);
        }

        private static string? FormatMessage(string? code, string? message)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return string.IsNullOrWhiteSpace(message) ? null : message;
            }

            return string.IsNullOrWhiteSpace(message)
                ? code
                : $"{code}: {message}";
        }
    }
}
