using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ShippingRates.ShippingProviders
{
    /// <summary>
    ///     Provides rates from DHL.
    /// </summary>
    public class DHLProvider : AbstractShippingProvider
    {
        public override string Name { get => "DHL"; }

        // These values need to stay in sync with the values in the "loadServiceCodes" method.

        public enum AvailableServices
        {
            NextDayAir = 1,
            SecondDayAir = 2,
            Ground = 4,
            WorldwideExpress = 8,
            WorldwideExpedited = 16,
            Standard = 32,
            ThreeDaySelect = 64,
            NextDayAirSaver = 128,
            NextDayAirEarlyAM = 256,
            WorldwideExpressPlus = 512,
            SecondDayAirAM = 1024,
            ExpressSaver = 2048,
            SurePost = 4096,
            All = 8191
        }
        private const int DefaultTimeout = 10;
        private const string TestServicesUrl = "http://xmlpitest-ea.dhl.com/XMLShippingServlet";
        private const string ProductionServicesUrl = "";
        private readonly string _siteId;
        private readonly string _password;
        private readonly int _timeout;
        private readonly bool _useProduction;
        private readonly Hashtable _serviceCodes = new Hashtable(12);

        public DHLProvider(string siteId, string password, bool useProduction) :
            this(siteId, password, useProduction, DefaultTimeout)
        {
        }

        public DHLProvider(string siteId, string password, bool useProduction, int timeout)
        {
            _siteId = siteId;
            _password = password;
            _timeout = timeout;
            _useProduction = useProduction;
            LoadServiceCodes();
        }

        public AvailableServices Services { get; set; } = AvailableServices.All;

        private Uri RatesUri => new Uri(_useProduction ? ProductionServicesUrl : TestServicesUrl);

        private string BuildRatesRequestMessage(DateTime messageTime, string messageReference)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new XmlTextWriter(memoryStream, Encoding.UTF8))
                {
                    //writer.WriteAttributeString("lang", "en-US");

                    writer.WriteStartDocument();
                    writer.WriteStartElement("GetQuote");

                    writer.WriteStartElement("Request");
                    writer.WriteStartElement("ServiceHeader");
                    writer.WriteElementString("MessageTime", messageTime.ToString("s", CultureInfo.CreateSpecificCulture("en-US")));
                    writer.WriteElementString("MessageReference", messageReference);
                    writer.WriteElementString("SiteID", _siteId);
                    writer.WriteElementString("Password", _password);
                    writer.WriteEndElement(); // </ServiceHeader>
                    writer.WriteEndElement(); // </Request>

                    writer.WriteStartElement("From");
                    writer.WriteElementString("CountryCode", Shipment.OriginAddress.CountryCode);
                    writer.WriteElementString("PostalCode", Shipment.OriginAddress.PostalCode);
                    writer.WriteEndElement(); // </From>

                    writer.WriteStartElement("PickupType");
                    writer.WriteElementString("Code", "03");
                    writer.WriteEndElement(); // </PickupType>

                    writer.WriteStartElement("CustomerClassification");
                    writer.WriteEndElement(); // </CustomerClassification

                    writer.WriteStartElement("Shipment");
                    writer.WriteStartElement("Shipper");
                    writer.WriteStartElement("Address");
                    writer.WriteElementString("PostalCode", Shipment.OriginAddress.PostalCode);
                    writer.WriteElementString("CountryCode", Shipment.OriginAddress.CountryCode);
                    writer.WriteEndElement(); // </Address>
                    writer.WriteEndElement(); // </Shipper>

                    writer.WriteStartElement("ShipTo");
                    writer.WriteStartElement("Address");
                    if (!string.IsNullOrWhiteSpace(Shipment.DestinationAddress.State))
                    {
                        writer.WriteElementString("StateProvinceCode", Shipment.DestinationAddress.State);
                    }
                    if (!string.IsNullOrWhiteSpace(Shipment.DestinationAddress.PostalCode))
                    {
                        writer.WriteElementString("PostalCode", Shipment.DestinationAddress.PostalCode);
                    }
                    writer.WriteElementString("CountryCode", Shipment.DestinationAddress.CountryCode);
                    if (Shipment.DestinationAddress.IsResidential)
                    {
                        writer.WriteElementString("ResidentialAddressIndicator", "true");
                    }
                    writer.WriteEndElement(); // </Address>
                    writer.WriteEndElement(); // </ShipTo>

                    if (Shipment.Options.ShippingDate != null)
                    {
                        writer.WriteStartElement("DeliveryTimeInformation");
                        writer.WriteElementString("PackageBillType", "03");
                        writer.WriteStartElement("Pickup");
                        writer.WriteElementString("Date", Shipment.Options.ShippingDate.Value.ToString("yyyyMMdd"));
                        writer.WriteElementString("Time", "1000");
                        writer.WriteEndElement();// </Pickup>
                        writer.WriteEndElement();// </DeliveryTimeInformation>
                    }
                    if (Shipment.Options.SaturdayDelivery)
                    {
                        writer.WriteStartElement("ShipmentServiceOptions");
                        writer.WriteElementString("SaturdayDelivery", "");
                        writer.WriteEndElement();// </ShipmentServiceOptions>
                    }

                    for (var i = 0; i < Shipment.Packages.Count; i++)
                    {
                        writer.WriteStartElement("Package");
                        writer.WriteStartElement("PackagingType");
                        writer.WriteElementString("Code", "02");
                        writer.WriteEndElement(); //</PackagingType>
                        writer.WriteStartElement("PackageWeight");
                        writer.WriteElementString("Weight", Shipment.Packages[i].RoundedWeight.ToString());
                        writer.WriteEndElement(); // </PackageWeight>
                        writer.WriteStartElement("Dimensions");
                        writer.WriteElementString("Length", Shipment.Packages[i].RoundedLength.ToString());
                        writer.WriteElementString("Width", Shipment.Packages[i].RoundedWidth.ToString());
                        writer.WriteElementString("Height", Shipment.Packages[i].RoundedHeight.ToString());
                        writer.WriteEndElement(); // </Dimensions>
                        writer.WriteStartElement("PackageServiceOptions");
                        writer.WriteStartElement("InsuredValue");
                        writer.WriteElementString("CurrencyCode", "USD");
                        writer.WriteElementString("MonetaryValue", Shipment.Packages[i].InsuredValue.ToString());
                        writer.WriteEndElement(); // </InsuredValue>

                        if (Shipment.Packages[i].SignatureRequiredOnDelivery)
                        {
                            writer.WriteStartElement("DeliveryConfirmation");
                            writer.WriteElementString("DCISType", "2");         // 2 represents Delivery Confirmation Signature Required
                            writer.WriteEndElement(); // </DeliveryConfirmation>
                        }

                        writer.WriteEndElement(); // </PackageServiceOptions>
                        writer.WriteEndElement(); // </Package>
                    }
                    writer.WriteEndElement();   // </Shipment>

                    writer.WriteEndElement();   // </GetQuote>
                    writer.WriteEndDocument();
                    writer.Flush();
                    writer.Close();
                }
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        public override async Task GetRates()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(_timeout);

                var request = BuildRatesRequestMessage(DateTime.Now, Guid.NewGuid().ToString());
                using (var httpContent = new StringContent(request, Encoding.UTF8, "text/xml"))
                {
                    var response = await httpClient.PostAsync(RatesUri, httpContent).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        var xDoc = XDocument.Load(await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
                        ParseRatesResponseMessage(xDoc);
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetServiceCodes()
        {
            if (_serviceCodes != null && _serviceCodes.Count > 0)
            {
                var serviceCodes = new Dictionary<string, string>();

                foreach (var serviceCodeKey in _serviceCodes.Keys)
                {
                    var serviceCode = (AvailableService)_serviceCodes[serviceCodeKey];
                    serviceCodes.Add((string)serviceCodeKey, serviceCode.Name);
                }

                return serviceCodes;
            }

            return null;
        }

        private void LoadServiceCodes()
        {
            _serviceCodes.Add("01", new AvailableService("UPS Next Day Air", 1));
            _serviceCodes.Add("02", new AvailableService("UPS Second Day Air", 2));
            _serviceCodes.Add("03", new AvailableService("UPS Ground", 4));
            _serviceCodes.Add("07", new AvailableService("UPS Worldwide Express", 8));
            _serviceCodes.Add("08", new AvailableService("UPS Worldwide Expedited", 16));
            _serviceCodes.Add("11", new AvailableService("UPS Standard", 32));
            _serviceCodes.Add("12", new AvailableService("UPS 3-Day Select", 64));
            _serviceCodes.Add("13", new AvailableService("UPS Next Day Air Saver", 128));
            _serviceCodes.Add("14", new AvailableService("UPS Next Day Air Early AM", 256));
            _serviceCodes.Add("54", new AvailableService("UPS Worldwide Express Plus", 512));
            _serviceCodes.Add("59", new AvailableService("UPS 2nd Day Air AM", 1024));
            _serviceCodes.Add("65", new AvailableService("UPS Express Saver", 2048));
            _serviceCodes.Add("93", new AvailableService("UPS Sure Post", 4096));
        }

        private void ParseRatesResponseMessage(XDocument xDoc)
        {
            if (xDoc.Root != null)
            {
                var ratedShipment = xDoc.Root.Elements("RatedShipment");
                foreach (var rateNode in ratedShipment)
                {
                    var name = rateNode.XPathSelectElement("Service/Code").Value;
                    AvailableService service;
                    if (_serviceCodes.ContainsKey(name))
                    {
                        service = (AvailableService) _serviceCodes[name];
                    }
                    else
                    {
                        continue;
                    }
                    if (((int) Services & service.EnumValue) != service.EnumValue)
                    {
                        continue;
                    }
                    var description = "";
                    if (_serviceCodes.ContainsKey(name))
                    {
                        description = _serviceCodes[name].ToString();
                    }
                    var totalCharges = Convert.ToDecimal(rateNode.XPathSelectElement("TotalCharges/MonetaryValue").Value);
                    /*if (UseNegotiatedRates)
                    {
                        var negotiatedRate = rateNode.XPathSelectElement("NegotiatedRates/NetSummaryCharges/GrandTotal/MonetaryValue");
                        if (negotiatedRate != null) // check for negotiated rate
                        {
                            totalCharges = Convert.ToDecimal(negotiatedRate.Value);
                        }
                    }*/

                    var date = rateNode.XPathSelectElement("GuaranteedDaysToDelivery").Value;
                    if (string.IsNullOrEmpty(date)) // no gauranteed delivery date, so use MaxDate to ensure correct sorting
                    {
                        date = DateTime.MaxValue.ToShortDateString();
                    }
                    else
                    {
                        date = (Shipment.Options.ShippingDate ?? DateTime.Now)
                            .AddDays(Convert.ToDouble(date)).ToShortDateString();
                    }
                    var deliveryTime = rateNode.XPathSelectElement("ScheduledDeliveryTime").Value;
                    if (string.IsNullOrEmpty(deliveryTime)) // no scheduled delivery time, so use 11:59:00 PM to ensure correct sorting
                    {
                        date += " 11:59:00 PM";
                    }
                    else
                    {
                        date += " " + deliveryTime.Replace("Noon", "PM").Replace("P.M.", "PM").Replace("A.M.", "AM");
                    }
                    var deliveryDate = DateTime.Parse(date);

                    AddRate(name, description, totalCharges, deliveryDate, new RateOptions()
                    {
                        SaturdayDelivery = Shipment.Options.SaturdayDelivery && deliveryDate.DayOfWeek == DayOfWeek.Saturday
                    });
                }
            }
        }

        private struct AvailableService
        {
            public readonly int EnumValue;
            public readonly string Name;

            public AvailableService(string name, int enumValue)
            {
                Name = name;
                EnumValue = enumValue;
            }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
