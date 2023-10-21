using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ShippingRates.Helpers.Extensions;
using ShippingRates.RateServiceWebReference;

namespace ShippingRates.ShippingProviders
{
    public abstract class FedExBaseProvider : AbstractShippingProvider
    {
        public bool UseNegotiatedRates { get; set; } = false;

        protected string AccountNumber { get; }
        protected string Key { get; }
        protected string MeterNumber { get; }
        protected string Password { get; }
        protected bool UseProduction { get; }
        protected abstract Dictionary<string, string> ServiceCodes { get; }

        /// <summary>
        ///     FedEx allows insured values for items being shipped except when utilizing SmartPost.
        ///     This setting will this value to be overwritten.
        /// </summary>
        protected bool _allowInsuredValues = true;

        public FedExBaseProvider(
            string key,
            string password,
            string accountNumber,
            string meterNumber,
            bool useProduction)
        {
            Key = key;
            Password = password;
            AccountNumber = accountNumber;
            MeterNumber = meterNumber;
            UseProduction = useProduction;
        }

        /// <summary>
        /// Gets service codes.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetServiceCodes() => (ServiceCodes?.Count ?? 0) > 0 ? ServiceCodes : null;

        /// <summary>
        /// Creates the rate request
        /// </summary>
        /// <returns></returns>
        protected RateRequest CreateRateRequest()
        {
            // Build the RateRequest
            var request = new RateRequest
            {
                WebAuthenticationDetail = new WebAuthenticationDetail
                {
                    UserCredential = new WebAuthenticationCredential
                    {
                        Key = Key,
                        Password = Password
                    }
                },
                ClientDetail = new ClientDetail
                {
                    AccountNumber = AccountNumber,
                    MeterNumber = MeterNumber
                },
                Version = new VersionId(),
                ReturnTransitAndCommit = true,
                ReturnTransitAndCommitSpecified = true,
                RequestedShipment = new RequestedShipment()
                {
                    ShipTimestamp = Shipment.Options.ShippingDate ?? DateTime.Now, // Shipping date and time
                    ShipTimestampSpecified = true,
                    DropoffType = DropoffType.REGULAR_PICKUP, //Drop off types are BUSINESS_SERVICE_CENTER, DROP_BOX, REGULAR_PICKUP, REQUEST_COURIER, STATION
                    DropoffTypeSpecified = true,
                    PackagingType = "YOUR_PACKAGING",
                    PackageCount = Shipment.PackageCount.ToString(),
                    RateRequestTypes = GetRateRequestTypes().ToArray(),
                    PreferredCurrency = Shipment.Options.GetCurrencyCode()
                }
            };

            if (Shipment.Options.FedExOneRate)
            {
                if (!string.IsNullOrEmpty(Shipment.Options.FedExOneRatePackageOverride))
                {
                    request.RequestedShipment.PackagingType = Shipment.Options.FedExOneRatePackageOverride;
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

            if (Shipment.Options.SaturdayDelivery)
            {
                request.VariableOptions = new[] { ServiceOptionType.SATURDAY_DELIVERY };
            }

            SetShipmentDetails(request);

            return request;
        }

        private IEnumerable<RateRequestType> GetRateRequestTypes()
        {
            yield return RateRequestType.LIST;
            if (!string.IsNullOrEmpty(Shipment.Options.PreferredCurrencyCode))
            {
                yield return RateRequestType.PREFERRED;
            }
        }

        /// <summary>
        /// Sets shipment details
        /// </summary>
        /// <param name="request"></param>
        protected abstract void SetShipmentDetails(RateRequest request);

        /// <summary>
        /// Gets rates
        /// </summary>
        public override async Task GetRates()
        {
            var request = CreateRateRequest();
            var service = new RatePortTypeClient(UseProduction);
            try
            {
                // Call the web service passing in a RateRequest and returning a RateReply
                var reply = await service.getRatesAsync(request).ConfigureAwait(false);

                if (reply.RateReply != null)
                {
                    ProcessReply(reply.RateReply);
                    ProcessErrors(reply.RateReply);
                }
                else
                {
                    AddInternalError($"FedEx provider: API returned NULL result");
                }
            }
            catch (Exception e)
            {
                AddInternalError($"FedEx provider exception: {e.Message}");
            }
        }

        /// <summary>
        /// Processes the reply
        /// </summary>
        /// <param name="reply"></param>
        protected void ProcessReply(RateReply reply)
        {
            if (reply?.RateReplyDetails == null)
                return;

            foreach (var rateReplyDetail in reply.RateReplyDetails)
            {
                var key = rateReplyDetail.ServiceType.ToString();

                if (!ServiceCodes.Keys.Contains(key))
                {
                    AddInternalError($"Unknown FedEx rate code: {key}");
                }
                else
                {
                    var rateDetails = GetRateDetailsByRateType(rateReplyDetail);
                    var rates = rateDetails.Select(r => GetCurrencyConvertedRate(r.ShipmentRateDetail));
                    rates = rates.Any(r => r.currencyCode == Shipment.Options.GetCurrencyCode())
                        ? rates.Where(r => r.currencyCode == Shipment.Options.GetCurrencyCode())
                        : rates;

                    var netCharge = rates.OrderByDescending(r => r.amount).FirstOrDefault();
                    var deliveryDate = rateReplyDetail.DeliveryTimestampSpecified ? rateReplyDetail.DeliveryTimestamp : DateTime.Now.AddDays(30);

                    AddRate(key, ServiceCodes[key], netCharge.amount, deliveryDate, new RateOptions()
                    {
                        SaturdayDelivery = rateReplyDetail.AppliedOptions?.Contains(ServiceOptionType.SATURDAY_DELIVERY) ?? false
                    },
                    netCharge.currencyCode);
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
                    (UseNegotiatedRates && negotiatedRateTypes.Contains(rsd.ShipmentRateDetail.RateType)) ||
                    (!UseNegotiatedRates && !negotiatedRateTypes.Contains(rsd.ShipmentRateDetail.RateType)))
                .ToArray();
        }

        private (decimal amount, string currencyCode) GetCurrencyConvertedRate(ShipmentRateDetail rateDetail)
        {
            var shipmentCurrencyCode = Shipment.Options.GetCurrencyCode();

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
        protected void SetDestination(RateRequest request)
        {
            request.RequestedShipment.Recipient = new Party
            {
                Address = Shipment.DestinationAddress.GetFedExAddress()
            };
        }

        /// <summary>
        /// Sets the origin
        /// </summary>
        /// <param name="request"></param>
        protected void SetOrigin(RateRequest request)
        {
            request.RequestedShipment.Shipper = new Party
            {
                Address = Shipment.OriginAddress.GetFedExAddress()
            };
        }

        /// <summary>
        /// Sets package line items
        /// </summary>
        /// <param name="request"></param>
        protected void SetPackageLineItems(RateRequest request)
        {
            request.RequestedShipment.RequestedPackageLineItems = new RequestedPackageLineItem[Shipment.PackageCount];

            var i = 0;
            foreach (var package in Shipment.Packages)
            {
                request.RequestedShipment.RequestedPackageLineItems[i] = new RequestedPackageLineItem()
                {
                    SequenceNumber = (i + 1).ToString(),
                    GroupPackageCount = "1",

                    // Package weight
                    Weight = new Weight()
                    {
                        Units = WeightUnits.LB,
                        UnitsSpecified = true,
                        Value = package.RoundedWeight,
                        ValueSpecified = true
                    },

                    // Package dimensions
                    Dimensions = new Dimensions()
                    {
                        Length = package.RoundedLength.ToString(),
                        Width = package.RoundedWidth.ToString(),
                        Height = package.RoundedHeight.ToString(),
                        Units = LinearUnits.IN,
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

        private void ProcessErrors(RateReply reply)
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
                    AddError(err);
                }
            }
        }
    }
}
