using ShippingRates.ShippingProviders.USPS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ShippingRates.ShippingProviders
{
    /// <summary>
    /// </summary>
    public class USPSProvider : USPSBaseProvider
    {
        /// <summary>
        /// If set to ALL, special service types will not be returned. This is a limitation of the USPS API.
        /// </summary>
        private readonly string _service;

        private readonly string _userId;

        private readonly SpecialServices[] _specialServices;

        /// <summary>
        /// Service codes. {0} is a placeholder for 1-Day, 2-Day, 3-Day, Military, DPO or a space
        /// </summary>
        private readonly Dictionary<string, string> _serviceCodes = new Dictionary<string, string>
        {
            {"First-Class Mail Large Envelope","First-Class Mail Large Envelope"},
            {"First-Class Mail Letter","First-Class Mail Letter"},
            {"First-Class Mail Parcel","First-Class Mail Parcel"},
            {"First-Class Mail Postcards","First-Class Mail Postcards"},
            {"Priority Mail {0}","Priority Mail {0}"},
            {"Priority Mail Express {0} Hold For Pickup","Priority Mail Express {0} Hold For Pickup"},
            {"Priority Mail Express {0}","Priority Mail Express {0}"},
            {"Standard Post","Standard Post"},
            {"Media Mail Parcel","Media Mail Parcel"},
            {"Library Mail Parcel","Library Mail Parcel"},
            {"Priority Mail Express {0} Flat Rate Envelope","Priority Mail Express {0} Flat Rate Envelope"},
            {"First-Class Mail Large Postcards","First-Class Mail Large Postcards"},
            {"Priority Mail {0} Flat Rate Envelope","Priority Mail {0} Flat Rate Envelope"},
            {"Priority Mail {0} Medium Flat Rate Box","Priority Mail {0} Medium Flat Rate Box"},
            {"Priority Mail {0} Large Flat Rate Box","Priority Mail {0} Large Flat Rate Box"},
            {"Priority Mail Express {0} Sunday/Holiday Delivery","Priority Mail Express {0} Sunday/Holiday Delivery"},
            {"Priority Mail Express {0} Sunday/Holiday Delivery Flat Rate Envelope","Priority Mail Express {0} Sunday/Holiday Delivery Flat Rate Envelope"},
            {"Priority Mail Express {0} Flat Rate Envelope Hold For Pickup","Priority Mail Express {0} Flat Rate Envelope Hold For Pickup"},
            {"Priority Mail {0} Small Flat Rate Box","Priority Mail {0} Small Flat Rate Box"},
            {"Priority Mail {0} Padded Flat Rate Envelope","Priority Mail {0} Padded Flat Rate Envelope"},
            {"Priority Mail Express {0} Legal Flat Rate Envelope","Priority Mail Express {0} Legal Flat Rate Envelope"},
            {"Priority Mail Express {0} Legal Flat Rate Envelope Hold For Pickup","Priority Mail Express {0} Legal Flat Rate Envelope Hold For Pickup"},
            {"Priority Mail Express {0} Sunday/Holiday Delivery Legal Flat Rate Envelope","Priority Mail Express {0} Sunday/Holiday Delivery Legal Flat Rate Envelope"},
            {"Priority Mail {0} Hold For Pickup","Priority Mail {0} Hold For Pickup"},
            {"Priority Mail {0} Large Flat Rate Box Hold For Pickup","Priority Mail {0} Large Flat Rate Box Hold For Pickup"},
            {"Priority Mail {0} Medium Flat Rate Box Hold For Pickup","Priority Mail {0} Medium Flat Rate Box Hold For Pickup"},
            {"Priority Mail {0} Small Flat Rate Box Hold For Pickup","Priority Mail {0} Small Flat Rate Box Hold For Pickup"},
            {"Priority Mail {0} Flat Rate Envelope Hold For Pickup","Priority Mail {0} Flat Rate Envelope Hold For Pickup"},
            {"Priority Mail {0} Gift Card Flat Rate Envelope","Priority Mail {0} Gift Card Flat Rate Envelope"},
            {"Priority Mail {0} Gift Card Flat Rate Envelope Hold For Pickup","Priority Mail {0} Gift Card Flat Rate Envelope Hold For Pickup"},
            {"Priority Mail {0} Window Flat Rate Envelope","Priority Mail {0} Window Flat Rate Envelope"},
            {"Priority Mail {0} Window Flat Rate Envelope Hold For Pickup","Priority Mail {0} Window Flat Rate Envelope Hold For Pickup"},
            {"Priority Mail {0} Small Flat Rate Envelope","Priority Mail {0} Small Flat Rate Envelope"},
            {"Priority Mail {0} Small Flat Rate Envelope Hold For Pickup","Priority Mail {0} Small Flat Rate Envelope Hold For Pickup"},
            {"Priority Mail {0} Legal Flat Rate Envelope","Priority Mail {0} Legal Flat Rate Envelope"},
            {"Priority Mail {0} Legal Flat Rate Envelope Hold For Pickup","Priority Mail {0} Legal Flat Rate Envelope Hold For Pickup"},
            {"Priority Mail {0} Padded Flat Rate Envelope Hold For Pickup","Priority Mail {0} Padded Flat Rate Envelope Hold For Pickup"},
            {"Priority Mail {0} Regional Rate Box A","Priority Mail {0} Regional Rate Box A"},
            {"Priority Mail {0} Regional Rate Box A Hold For Pickup","Priority Mail {0} Regional Rate Box A Hold For Pickup"},
            {"Priority Mail {0} Regional Rate Box B","Priority Mail {0} Regional Rate Box B"},
            {"Priority Mail {0} Regional Rate Box B Hold For Pickup","Priority Mail {0} Regional Rate Box B Hold For Pickup"},
            {"First-Class Package Service Hold For Pickup","First-Class Package Service Hold For Pickup"},
            {"Priority Mail Express {0} Flat Rate Boxes","Priority Mail Express {0} Flat Rate Boxes"},
            {"Priority Mail Express {0} Flat Rate Boxes Hold For Pickup","Priority Mail Express {0} Flat Rate Boxes Hold For Pickup"},
            {"Priority Mail Express {0} Sunday/Holiday Delivery Flat Rate Boxes","Priority Mail Express {0} Sunday/Holiday Delivery Flat Rate Boxes"},
            {"Priority Mail {0} Regional Rate Box C","Priority Mail {0} Regional Rate Box C"},
            {"Priority Mail {0} Regional Rate Box C Hold For Pickup","Priority Mail {0} Regional Rate Box C Hold For Pickup"},
            {"First-Class Package Service","First-Class Package Service"},
            {"Priority Mail Express {0} Padded Flat Rate Envelope","Priority Mail Express {0} Padded Flat Rate Envelope"},
            {"Priority Mail Express {0} Padded Flat Rate Envelope Hold For Pickup","Priority Mail Express {0} Padded Flat Rate Envelope Hold For Pickup"},
            {"Priority Mail Express {0} Sunday/Holiday Delivery Padded Flat Rate Envelope","Priority Mail Express {0} Sunday/Holiday Delivery Padded Flat Rate Envelope"}
        };

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        public USPSProvider(string userId, string service = Services.All) :
            this(new USPSProviderConfiguration(userId) { Service = service })
        {
        }

        public USPSProvider(USPSProviderConfiguration configuration)
        {
            configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userId = configuration.UserId;
            _service = configuration.Service;
            _specialServices = configuration.SpecialServices;

            if (string.IsNullOrEmpty(_service))
            {
                _service = Services.All;
            }
        }

        [Obsolete("Please use ShipmentOptions instead for the shipDate, this constructor will be removed in the future")]
        public USPSProvider(string userId, string service, string shipDate)
            : this(userId, service)
        {
            Shipment.Options.ShippingDate = DateTime.Parse(shipDate);
        }

        /// <summary>
        /// Returns the supported service codes
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetServiceCodes()
        {
            if (_serviceCodes != null && _serviceCodes.Count > 0)
            {
                var serviceCodes = new Dictionary<string, string>();
                var variableValues = new List<string>() {"1-Day", "2-Day", "3-Day", "Military", "DPO"};

                foreach (var variableValue in variableValues)
                {
                    foreach (var serviceCodeKey in _serviceCodes.Keys)
                    {
                        var serviceCode = _serviceCodes[serviceCodeKey];
                        var swappedServiceCodeKey = serviceCodeKey.Replace("{0}", variableValue);
                        var swappedServiceCode = serviceCode.Replace("{0}", variableValue);
                        
                        if (!serviceCodes.ContainsKey(swappedServiceCode))
                            serviceCodes.Add(swappedServiceCodeKey, swappedServiceCode);
                    }
                }

                return serviceCodes;
            }

            return null;
        }

        public override async Task GetRates()
        {
            await GetRates(false).ConfigureAwait(false);
        }

        public async Task GetRates(bool baseRatesOnly)
        {
            // USPS only available for domestic addresses. International is a different API.
            if (!IsDomesticUSPSAvailable())
            {
                return;
            }

            var sb = new StringBuilder();
            var specialServices = GetSpecialServicesForShipment(Shipment);

            var settings = new XmlWriterSettings
            {
                Indent = false,
                OmitXmlDeclaration = true,
                NewLineHandling = NewLineHandling.None
            };

            using (var writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartElement("RateV4Request");
                writer.WriteAttributeString("USERID", _userId);
                if (!baseRatesOnly)
                {
                    writer.WriteElementString("Revision", "2");
                }
                var i = 0;
                foreach (var package in Shipment.Packages)
                {
                    string size;
                    var container = package.Container;
                    if (IsPackageLarge(package))
                    {
                        size = "LARGE";
                        // Container must be RECTANGULAR or NONRECTANGULAR when SIZE is LARGE
                        if (container == null || container.ToUpperInvariant() != "NONRECTANGULAR")
                        {
                            container = "RECTANGULAR";
                        }
                    }
                    else
                    {
                        size = "REGULAR";
                        if (container == null)
                        {
                            container = string.Empty;
                        }
                    }

                    writer.WriteStartElement("Package");
                    writer.WriteAttributeString("ID", i.ToString());
                    writer.WriteElementString("Service", _service);
                    writer.WriteElementString("ZipOrigination", Shipment.OriginAddress.CountryCode == "US" && Shipment.OriginAddress.PostalCode.Length > 5? Shipment.OriginAddress.PostalCode.Substring(0, 5) : Shipment.OriginAddress.PostalCode);
                    writer.WriteElementString("ZipDestination", Shipment.DestinationAddress.CountryCode == "US" && Shipment.DestinationAddress.PostalCode.Length > 5 ? Shipment.DestinationAddress.PostalCode.Substring(0, 5) : Shipment.DestinationAddress.PostalCode);
                    writer.WriteElementString("Pounds", package.PoundsAndOunces.Pounds.ToString());
                    writer.WriteElementString("Ounces", package.PoundsAndOunces.Ounces.ToString());

                    writer.WriteElementString("Container", container);
                    writer.WriteElementString("Size", size);
                    writer.WriteElementString("Width", package.RoundedWidth.ToString());
                    writer.WriteElementString("Length", package.RoundedLength.ToString());
                    writer.WriteElementString("Height", package.RoundedHeight.ToString());
                    writer.WriteElementString("Girth", package.CalculatedGirth.ToString());
                    writer.WriteElementString("Value", package.InsuredValue.ToString());
                    if (RequiresMachinable(_service))
                    {
                        writer.WriteElementString("Machinable", IsPackageMachinable(package).ToString());
                    }
                    if (Shipment.Options.ShippingDate != null)
                    {
                        writer.WriteElementString("ShipDate",
                            Shipment.Options.ShippingDate.Value.ToString("yyyy-MM-dd"));
                    }

                    if (AllowsSpecialServices(_service) && specialServices.Any())
                    {
                        writer.WriteStartElement("SpecialServices");
                        foreach (var service in specialServices)
                        {
                            writer.WriteElementString("SpecialService", ((int)service).ToString());
                        }
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    i++;
                }

                writer.WriteEndElement();
                writer.Flush();
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var rateUri = new Uri($"{ProductionUrl}?API=RateV4&XML={sb}");
                    var response = await httpClient.GetStringAsync(rateUri).ConfigureAwait(false);

                    ParseResult(response, specialServices);
                }
            }
            catch (Exception ex)
            {
                AddInternalError($"USPS provider exception: {ex.Message}");
            }
        }

        public List<SpecialServices> GetSpecialServicesForShipment(Shipment shipment)
        {
            shipment = shipment ?? throw new ArgumentNullException(nameof(shipment));
            var shipmentSpecialServices = new List<SpecialServices>(_specialServices ?? Array.Empty<SpecialServices>());

            if (shipment.Packages.Any(p => p.SignatureRequiredOnDelivery))
            {
                shipmentSpecialServices.Add(SpecialServices.AdultSignatureRequired);
                shipmentSpecialServices.Add(SpecialServices.CertifiedMailAdultSignatureRequired);
            }

            if (shipment.Packages.Any(p => p.InsuredValue > 0))
            {
                shipmentSpecialServices.Add(SpecialServices.Insurance);
                shipmentSpecialServices.Add(SpecialServices.InsurancePriorityMail);
                shipmentSpecialServices.Add(SpecialServices.InsurancePriorityMailExpress);
            }

            return shipmentSpecialServices;
        }

        public bool IsDomesticUSPSAvailable()
        {
            return Shipment.OriginAddress.IsUnitedStatesAddress() && Shipment.DestinationAddress.IsUnitedStatesAddress();
        }

        public static bool IsPackageLarge(Package package)
        {
            package = package ?? throw new ArgumentNullException(nameof(package));
            return package.IsOversize || package.Width > 12 || package.Length > 12 || package.Height > 12;
        }

        public static bool IsPackageMachinable(Package package)
        {
            package = package ?? throw new ArgumentNullException(nameof(package));
            // Machinable parcels cannot be larger than 27 x 17 x 17 and cannot weight more than 25 lbs.
            if (package.Weight > 25)
            {
                return false;
            }

            return (package.Width <= 27 && package.Height <= 17 && package.Length <= 17) || (package.Width <= 17 && package.Height <= 27 && package.Length <= 17) || (package.Width <= 17 && package.Height <= 17 && package.Length <= 27);
        }

        private void ParseResult(string response, IList<SpecialServices> includeSpecialServiceCodes = null)
        {
            var document = XElement.Parse(response, LoadOptions.None);

            var rates = from item in document.Descendants("Postage")
                group item by (string) item.Element("MailService")
                into g
                select new {Name = g.Key,
                            TotalCharges = g.Sum(x => decimal.Parse((string) x.Element("Rate"))),
                            TotalCommercialCharges = g.Sum(x => decimal.Parse((string) x.Element("CommercialRate") ?? "0")),
                            DeliveryDate = g.Select(x => (string) x.Element("CommitmentDate")).FirstOrDefault(),
                            SpecialServices = g.Select(x => x.Element("SpecialServices")).FirstOrDefault() };

            foreach (var r in rates)
            {
                //string name = r.Name.Replace(REMOVE_FROM_RATE_NAME, string.Empty);
                var name = Regex.Replace(r.Name, "&lt.*&gt;", "");
                var additionalCharges = 0.0m;

                if (includeSpecialServiceCodes != null && includeSpecialServiceCodes.Count > 0 && r.SpecialServices != null)
                {
                    var specialServices = r.SpecialServices.XPathSelectElements("SpecialService").ToList();
                    if (specialServices.Count > 0)
                    {
                        foreach (var specialService in specialServices)
                        {
                            var serviceId = (int)specialService.Element("ServiceID");
                            var price = decimal.Parse((string) specialService.Element("Price"));

                            if (includeSpecialServiceCodes.Contains((SpecialServices)serviceId))
                                additionalCharges += price;
                        }
                    }
                }

                var isNegotiatedRate = _service == Services.Online && r.TotalCommercialCharges > 0;
                var totalCharges = isNegotiatedRate ? r.TotalCommercialCharges : r.TotalCharges;

                if (r.DeliveryDate != null && DateTime.TryParse(r.DeliveryDate, out DateTime deliveryDate))
                {
                    var rateOptions = new RateOptions()
                    {
                        SaturdayDelivery = Shipment.Options.SaturdayDelivery && deliveryDate.DayOfWeek == DayOfWeek.Saturday
                    };
                    AddRate(name, string.Concat("USPS ", name), totalCharges + additionalCharges, deliveryDate, rateOptions, USPSCurrencyCode);
                }
                else
                {
                    AddRate(name, string.Concat("USPS ", name), totalCharges + additionalCharges, DateTime.Now.AddDays(30), null, USPSCurrencyCode);
                }
            }

            //check for errors
            ParseErrors(document);
        }

        public static bool RequiresMachinable(string service)
        {
            return
                service == Services.FirstClass || // TODO: Check for (FirstClassMailType = 'LETTER' or FirstClassMailType = 'FLAT')]
                service == Services.All ||
                service == Services.Online;
        }

        public static bool AllowsSpecialServices(string service)
        {
            return service != Services.All && service != Services.Online && service != Services.Plus;
        }
    }
}
