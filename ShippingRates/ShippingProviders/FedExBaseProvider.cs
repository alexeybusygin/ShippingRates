using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ShippingRates.RateServiceWebReference;

namespace ShippingRates.ShippingProviders
{
    public abstract class FedExBaseProvider : AbstractShippingProvider
    {
        protected string _accountNumber;
        protected string _key;
        protected string _meterNumber;
        protected string _password;
        protected bool _useProduction = true;
        protected Dictionary<string, string> _serviceCodes;

        /// <summary>
        ///     FedEx allows insured values for items being shipped except when utilizing SmartPost.
        ///     This setting will this value to be overwritten.
        /// </summary>
        protected bool _allowInsuredValues = true;

        /// <summary>
        /// Sets service codes.
        /// </summary>
        protected abstract void SetServiceCodes();

        /// <summary>
        /// Gets service codes.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetServiceCodes()
        {
            if (_serviceCodes != null && _serviceCodes.Count > 0)
            {
                return new Dictionary<string, string>(_serviceCodes);
            }

            return null;
        }

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
                        Key = _key,
                        Password = _password
                    }
                },
                ClientDetail = new ClientDetail
                {
                    AccountNumber = _accountNumber,
                    MeterNumber = _meterNumber
                },
                Version = new VersionId(),
                ReturnTransitAndCommit = true,
                ReturnTransitAndCommitSpecified = true
            };

            SetShipmentDetails(request);

            return request;
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
            var service = new RatePortTypeClient(_useProduction);
            try
            {
                // Call the web service passing in a RateRequest and returning a RateReply
                var reply = await service.getRatesAsync(request);
                //
                if (reply.RateReply.HighestSeverity == NotificationSeverityType.SUCCESS ||
                    reply.RateReply.HighestSeverity == NotificationSeverityType.NOTE ||
                    reply.RateReply.HighestSeverity == NotificationSeverityType.WARNING)
                {
                    ProcessReply(reply.RateReply);
                }
                ProcessErrors(reply.RateReply);
                ShowNotifications(reply.RateReply);
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
            foreach (var rateReplyDetail in reply.RateReplyDetails)
            {
                var netCharge = rateReplyDetail.RatedShipmentDetails.Max(x => x.ShipmentRateDetail.TotalNetCharge.Amount);

                var key = rateReplyDetail.ServiceType.ToString();
                var deliveryDate = rateReplyDetail.DeliveryTimestampSpecified ? rateReplyDetail.DeliveryTimestamp : DateTime.Now.AddDays(30);

                if (!_serviceCodes.Keys.Contains(key))
                {
                    AddInternalError($"Unknown FedEx rate code: {key}");
                }
                else
                {
                    AddRate(key, _serviceCodes[key], netCharge, deliveryDate);
                }
            }
        }

        /// <summary>
        /// Sets the destination
        /// </summary>
        /// <param name="request"></param>
        protected void SetDestination(RateRequest request)
        {
            request.RequestedShipment.Recipient = new Party
            {
                Address = new RateServiceWebReference.Address
                {
                    StreetLines = new string[1] { "" },
                    City = "",
                    StateOrProvinceCode = "",
                    PostalCode = Shipment.DestinationAddress.PostalCode,
                    CountryCode = Shipment.DestinationAddress.CountryCode,
                    Residential = Shipment.DestinationAddress.IsResidential,
                    ResidentialSpecified = Shipment.DestinationAddress.IsResidential
                }
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
                Address = new RateServiceWebReference.Address
                {
                    StreetLines = new string[1] { "" },
                    City = "",
                    StateOrProvinceCode = "",
                    PostalCode = Shipment.OriginAddress.PostalCode,
                    CountryCode = Shipment.OriginAddress.CountryCode,
                    Residential = Shipment.OriginAddress.IsResidential,
                    ResidentialSpecified = Shipment.OriginAddress.IsResidential
                }
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
                        AmountSpecified = true,
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

        /// <summary>
        /// Outputs the notifications to the debug console
        /// </summary>
        /// <param name="reply"></param>
        protected static void ShowNotifications(RateReply reply)
        {
            Debug.WriteLine("Notifications");
            for (var i = 0; i < reply.Notifications.Length; i++)
            {
                var notification = reply.Notifications[i];
                Debug.WriteLine("Notification no. {0}", i);
                Debug.WriteLine(" Severity: {0}", notification.Severity);
                Debug.WriteLine(" Code: {0}", notification.Code);
                Debug.WriteLine(" Message: {0}", notification.Message);
                Debug.WriteLine(" Source: {0}", notification.Source);
            }
        }

        private void ProcessErrors(RateReply reply)
        {
            var errorTypes = new NotificationSeverityType[]
            {
                NotificationSeverityType.ERROR,
                NotificationSeverityType.FAILURE
            };

            if (reply.Notifications != null && reply.Notifications.Any())
            {
                var errors = reply.Notifications
                    .Where(e => !e.SeveritySpecified || errorTypes.Contains(e.Severity))
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
