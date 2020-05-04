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
            if (reply.RateReplyDetails == null)
                return;

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
                Address = GetFedExAddress(Shipment.DestinationAddress)
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
                Address = GetFedExAddress(Shipment.OriginAddress)
            };
        }

        private RateServiceWebReference.Address GetFedExAddress(Address address)
        {
            return new RateServiceWebReference.Address
            {
                StreetLines = GetStreetLines(address),
                City = address.City?.Trim(),
                StateOrProvinceCode = address.State?.Trim(),
                PostalCode = address.PostalCode?.Trim(),
                CountryCode = address.CountryCode?.Trim(),
                Residential = address.IsResidential,
                ResidentialSpecified = address.IsResidential,
            };
        }

        /// <summary>
        /// Get street lines array
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private string[] GetStreetLines(Address address)
        {
            var streetLines = new List<string>
            {
                address.Line1?.Trim(),
                address.Line2?.Trim(),
                address.Line3?.Trim()
            };
            streetLines = streetLines.Where(l => !string.IsNullOrEmpty(l)).ToList();
            return streetLines.Any() ? streetLines.ToArray() : new string[] { "" };
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
