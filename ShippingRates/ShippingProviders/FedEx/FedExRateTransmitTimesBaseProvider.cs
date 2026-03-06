using Microsoft.Extensions.Logging;
using ShippingRates.Helpers.Extensions;
using ShippingRates.Models;
using ShippingRates.OpenApi.FedEx.RateTransitTimes;
using ShippingRates.Services.FedEx;
using System;
using System.Collections.Generic;
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
                    PickupType = RequestedShipmentPickupType.USE_SCHEDULED_PICKUP,
                    RateRequestType = [.. GetRateRequestTypes(shipment)],
                    PreferredCurrency = shipment.Options.GetCurrencyCode(),
                    PackagingType = "YOUR_PACKAGING",
                    TotalPackageCount = shipment.Packages.Count,
                },
            };

            if (shipment.Options.ShippingDate != null)
            {
                request.RequestedShipment.ShipDateStamp = shipment.Options.ShippingDate.Value.ToString("yyyy-MM-dd");
            }

            if (shipment.Options.FedExOneRate)
            {
                if (!string.IsNullOrEmpty(shipment.Options.FedExOneRatePackageOverride))
                {
                    request.RequestedShipment.PackagingType = shipment.Options.FedExOneRatePackageOverride;
                }
                else
                {
                    request.RequestedShipment.PackagingType = "FEDEX_MEDIUM_BOX";
                }
                request.RequestedShipment.ShipmentSpecialServices = new RequestedShipmentSpecialServicesRequested()
                {
                    SpecialServiceTypes = ["FEDEX_ONE_RATE"]
                };
            }

            if (shipment.Options.SaturdayDelivery)
            {
                request.RateRequestControlParameters = new RateRequestControlParameters
                {
                    ReturnTransitTimes = true,
                    VariableOptions = RateRequestControlParametersVariableOptions.SATURDAY_DELIVERY
                };
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

            var httpClient = IsExternalHttpClient ? HttpClient : new HttpClient();

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
            finally
            {
                if (!IsExternalHttpClient && httpClient != null)
                    httpClient.Dispose();
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
            var msg = exception.Message;
            if (exception is ApiException<ErrorResponseVO> typedResponseException)
            {
                if (typedResponseException.Result?.Errors != null)
                {
                    msg = string.Join(", ", typedResponseException.Result.Errors.Select(e => e.Code));
                }
            }
            else if (exception is ApiException<ErrorResponseVO401> typedResponseException401)
            {
                if (typedResponseException401.Result?.Errors != null)
                {
                    msg = string.Join(", ", typedResponseException401.Result.Errors.Select(e => e.Code));
                }
            }
            else if (exception is ApiException<ErrorResponseVO403> typedResponseException403)
            {
                if (typedResponseException403.Result?.Errors != null)
                {
                    msg = string.Join(", ", typedResponseException403.Result.Errors.Select(e => e.Code));
                }
            }
            else if (exception is ApiException<ErrorResponseVO404> typedResponseException404)
            {
                if (typedResponseException404.Result?.Errors != null)
                {
                    msg = string.Join(", ", typedResponseException404.Result.Errors.Select(e => e.Code));
                }
            }
            else if (exception is ApiException<ErrorResponseVO500> typedResponseException500)
            {
                if (typedResponseException500.Result?.Errors != null)
                {
                    msg = string.Join(", ", typedResponseException500.Result.Errors.Select(e => e.Code));
                }
            }
            else if (exception is ApiException<ErrorResponseVO503> typedResponseException503)
            {
                if (typedResponseException503.Result?.Errors != null)
                {
                    msg = string.Join(", ", typedResponseException503.Result.Errors.Select(e => e.Code));
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
}
