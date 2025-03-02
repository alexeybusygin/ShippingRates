using ShippingRates.Helpers.Extensions;
using ShippingRates.Models;
using ShippingRates.RateServiceWebReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShippingRates.ShippingProviders
{
    public abstract class FedExBaseProvider : AbstractShippingProvider
    {
        [Obsolete("Please use FedExProviderConfiguration.UseNegotiatedRates instead")]
        public bool UseNegotiatedRates
        {
            get => _configuration.UseNegotiatedRates;
            set => _configuration.UseNegotiatedRates = value;
        }

        protected abstract Dictionary<string, string> ServiceCodes { get; }

        protected readonly FedExProviderConfiguration _configuration;

        public FedExBaseProvider(FedExProviderConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        ///     FedEx allows insured values for items being shipped except when utilizing SmartPost.
        ///     This setting will this value to be overwritten.
        /// </summary>
        protected bool _allowInsuredValues = true;

        /// <summary>
        /// Gets service codes.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetServiceCodes() => (ServiceCodes?.Count ?? 0) > 0 ? ServiceCodes : null;

        /// <summary>
        /// Creates the rate request
        /// </summary>
        /// <returns></returns>
        protected RateRequest CreateRateRequest(Shipment shipment)
        {
            // Build the RateRequest
            var request = new RateRequest
            {
                WebAuthenticationDetail = new WebAuthenticationDetail
                {
                    UserCredential = new WebAuthenticationCredential
                    {
                        Key = _configuration.Key,
                        Password = _configuration.Password
                    }
                },
                ClientDetail = new ClientDetail
                {
                    AccountNumber = _configuration.AccountNumber,
                    MeterNumber = _configuration.MeterNumber
                },
                Version = new VersionId(),
                ReturnTransitAndCommit = true,
                ReturnTransitAndCommitSpecified = true,
                RequestedShipment = new RequestedShipment()
                {
                    ShipTimestamp = shipment.Options.ShippingDate ?? DateTime.Now, // Shipping date and time
                    ShipTimestampSpecified = true,
                    DropoffType = DropoffType.REGULAR_PICKUP, //Drop off types are BUSINESS_SERVICE_CENTER, DROP_BOX, REGULAR_PICKUP, REQUEST_COURIER, STATION
                    DropoffTypeSpecified = true,
                    PackagingType = "YOUR_PACKAGING",
                    PackageCount = shipment.PackageCount.ToString(),
                    RateRequestTypes = GetRateRequestTypes(shipment).ToArray(),
                    PreferredCurrency = shipment.Options.GetCurrencyCode()
                }
            };

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
                request.RequestedShipment.SpecialServicesRequested = new ShipmentSpecialServicesRequested()
                {
                    SpecialServiceTypes = new[] { "FEDEX_ONE_RATE" }
                };
            }

            if (shipment.Options.SaturdayDelivery)
            {
                request.VariableOptions = new[] { ServiceOptionType.SATURDAY_DELIVERY };
            }

            SetShipmentDetails(request, shipment);

            return request;
        }

        private static IEnumerable<RateRequestType> GetRateRequestTypes(Shipment shipment)
        {
            yield return RateRequestType.LIST;
            if (!string.IsNullOrEmpty(shipment.Options.PreferredCurrencyCode))
            {
                yield return RateRequestType.PREFERRED;
            }
        }

        /// <summary>
        /// Sets shipment details
        /// </summary>
        /// <param name="request"></param>
        protected abstract void SetShipmentDetails(RateRequest request, Shipment shipment);

        /// <summary>
        /// Gets rates
        /// </summary>
        public override async Task<RateResult> GetRatesAsync(Shipment shipment)
        {
            var request = CreateRateRequest(shipment);
            var service = new RatePortTypeClient(_configuration.UseProduction);
            var resultBuilder = new RateResultBuilder(Name);

            try
            {
                // Call the web service passing in a RateRequest and returning a RateReply
                var reply = await service.getRatesAsync(request).ConfigureAwait(false);

                if (reply.RateReply != null)
                {
                    ProcessReply(shipment, reply.RateReply, resultBuilder);
                    ProcessErrors(reply.RateReply, resultBuilder);
                }
                else
                {
                    resultBuilder.AddInternalError($"FedEx provider: API returned NULL result");
                }
            }
            catch (Exception e)
            {
                resultBuilder.AddInternalError($"FedEx provider exception: {e.Message}");
            }

            return resultBuilder.GetRateResult();
        }

        /// <summary>
        /// Processes the reply
        /// </summary>
        /// <param name="reply"></param>
        private void ProcessReply(Shipment shipment, RateReply reply, RateResultBuilder resultBuilder)
        {
            if (reply?.RateReplyDetails == null)
                return;

            foreach (var rateReplyDetail in reply.RateReplyDetails)
            {
                var key = rateReplyDetail.ServiceType.ToString();

                if (ServiceCodes.TryGetValue(key, out string value))
                {
                    var rateDetails = GetRateDetailsByRateType(rateReplyDetail);
                    var rates = rateDetails.Select(r => FedExBaseProvider.GetCurrencyConvertedRate(shipment, r.ShipmentRateDetail));
                    rates = rates.Any(r => r.currencyCode == shipment.Options.GetCurrencyCode())
                        ? rates.Where(r => r.currencyCode == shipment.Options.GetCurrencyCode())
                        : rates;

                    var netCharge = rates.OrderByDescending(r => r.amount).FirstOrDefault();
                    var deliveryDate = rateReplyDetail.DeliveryTimestampSpecified ? rateReplyDetail.DeliveryTimestamp : DateTime.Now.AddDays(30);

                    resultBuilder.AddRate(key, value, netCharge.amount, deliveryDate, new RateOptions()
                    {
                        SaturdayDelivery = rateReplyDetail.AppliedOptions?.Contains(ServiceOptionType.SATURDAY_DELIVERY) ?? false
                    },
                    netCharge.currencyCode);
                }
                else
                {
                    resultBuilder.AddInternalError($"Unknown FedEx rate code: {key}");
                }
            }
        }

        private RatedShipmentDetail[] GetRateDetailsByRateType(RateReplyDetail rateReplyDetail)
        {
            var negotiatedRateTypes = new ReturnedRateType[]
            {
                ReturnedRateType.PAYOR_ACCOUNT_PACKAGE,
                ReturnedRateType.PAYOR_ACCOUNT_SHIPMENT,
                ReturnedRateType.NEGOTIATED
            };

            return rateReplyDetail.RatedShipmentDetails
                .Where(rsd =>
                    (_configuration.UseNegotiatedRates && negotiatedRateTypes.Contains(rsd.ShipmentRateDetail.RateType)) ||
                    (!_configuration.UseNegotiatedRates && !negotiatedRateTypes.Contains(rsd.ShipmentRateDetail.RateType)))
                .ToArray();
        }

        private static (decimal amount, string currencyCode) GetCurrencyConvertedRate(Shipment shipment, ShipmentRateDetail rateDetail)
        {
            var shipmentCurrencyCode = shipment.Options.GetCurrencyCode();

            if (rateDetail?.TotalNetCharge == null)
                return (0, shipmentCurrencyCode);

            var needCurrencyConversion = rateDetail.TotalNetCharge.Currency != shipmentCurrencyCode;
            if (!needCurrencyConversion)
                return (rateDetail.TotalNetCharge.Amount, shipmentCurrencyCode);

            var canConvertCurrency = (rateDetail.CurrencyExchangeRate?.RateSpecified ?? false)
                && rateDetail.TotalNetCharge.Currency == rateDetail.CurrencyExchangeRate.IntoCurrency
                && shipmentCurrencyCode == rateDetail.CurrencyExchangeRate.FromCurrency
                && rateDetail.CurrencyExchangeRate.Rate != 1
                && rateDetail.CurrencyExchangeRate.Rate != 0;

            if (!canConvertCurrency)
                return (rateDetail.TotalNetCharge.Amount, rateDetail.TotalNetCharge.Currency);

            return (Math.Round(rateDetail.TotalNetCharge.Amount / rateDetail.CurrencyExchangeRate.Rate, 2), shipmentCurrencyCode);
        }

        /// <summary>
        /// Sets the destination
        /// </summary>
        /// <param name="request"></param>
        protected static void SetDestination(RateRequest request, Shipment shipment)
        {
            request.RequestedShipment.Recipient = new Party
            {
                Address = shipment.DestinationAddress.GetFedExAddress()
            };
        }

        /// <summary>
        /// Sets the origin
        /// </summary>
        /// <param name="request"></param>
        protected static void SetOrigin(RateRequest request, Shipment shipment)
        {
            request.RequestedShipment.Shipper = new Party
            {
                Address = shipment.OriginAddress.GetFedExAddress()
            };
        }

        /// <summary>
        /// Sets package line items
        /// </summary>
        /// <param name="request"></param>
        protected void SetPackageLineItems(RateRequest request, Shipment shipment)
        {
            request.RequestedShipment.RequestedPackageLineItems = new RequestedPackageLineItem[shipment.PackageCount];

            var i = 0;
            foreach (var package in shipment.Packages)
            {
                request.RequestedShipment.RequestedPackageLineItems[i] = new RequestedPackageLineItem()
                {
                    SequenceNumber = (i + 1).ToString(),
                    GroupPackageCount = "1",

                    // Package weight
                    Weight = new Weight()
                    {
                        Units = WeightUnits.LB,                         // TODO: Add support for CM and KG
                        UnitsSpecified = true,
                        Value = shipment.Packages[i].GetRoundedWeight(UnitsSystem.USCustomary),
                        ValueSpecified = true
                    },

                    // Package dimensions
                    Dimensions = new Dimensions()
                    {
                        Length = shipment.Packages[i].GetRoundedLength(UnitsSystem.USCustomary).ToString(),
                        Width = shipment.Packages[i].GetRoundedWidth(UnitsSystem.USCustomary).ToString(),
                        Height = shipment.Packages[i].GetRoundedHeight(UnitsSystem.USCustomary).ToString(),
                        Units = LinearUnits.IN,                         // TODO: Add support for CM and KG
                        UnitsSpecified = true
                    }
                };

                if (_allowInsuredValues)
                {
                    // package insured value
                    request.RequestedShipment.RequestedPackageLineItems[i].InsuredValue = new Money
                    {
                        Amount = package.InsuredValue,
                        AmountSpecified = package.InsuredValue > 0,
                        Currency = "USD"
                    };
                }

                if (package.SignatureRequiredOnDelivery)
                {
                    var signatureOptionDetail = new SignatureOptionDetail { OptionType = SignatureOptionType.DIRECT };
                    var specialServicesRequested = new PackageSpecialServicesRequested() { SignatureOptionDetail = signatureOptionDetail };

                    request.RequestedShipment.RequestedPackageLineItems[i].SpecialServicesRequested = specialServicesRequested;
                }

                i++;
            }
        }

        private static void ProcessErrors(RateReply reply, RateResultBuilder resultBuilder)
        {
            var errorTypes = new NotificationSeverityType[]
            {
                NotificationSeverityType.ERROR,
                NotificationSeverityType.FAILURE
            };

            var noReplyDetails = reply.RateReplyDetails == null;

            if (reply.Notifications != null && reply.Notifications.Any())
            {
                var errors = reply.Notifications
                    .Where(e => !e.SeveritySpecified || errorTypes.Contains(e.Severity) || noReplyDetails)
                    .Select(error =>
                    new Error
                    {
                        Description = error.Message,
                        Source = error.Source,
                        Number = error.Code
                    });

                foreach (var err in errors)
                {
                    resultBuilder.AddError(err);
                }
            }
        }
    }
}
