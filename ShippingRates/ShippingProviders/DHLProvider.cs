using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

        public static Dictionary<char, string> AvailableServices => new Dictionary<char, string>
        {
            { '1', "EXPRESS DOMESTIC 12:00" },
            { '4', "JETLINE" },
            { '5', "SPRINTLINE" },
            { '7', "EXPRESS EASY" },
            { '8', "EXPRESS EASY" },
            { 'B', "EXPRESS BREAKBULK" },
            { 'C', "MEDICAL EXPRESS" },
            { 'D', "EXPRESS WORLDWIDE" },
            { 'E', "EXPRESS 9:00" },
            { 'F', "FREIGHT WORLDWIDE" },
            { 'G', "DOMESTIC ECONOMY SELECT" },
            { 'H', "ECONOMY SELECT" },
            { 'I', "EXPRESS DOMESTIC 9:00" },
            { 'J', "JUMBO BOX" },
            { 'K', "EXPRESS 9:00" },
            { 'L', "EXPRESS 10:30" },
            { 'M', "EXPRESS 10:30" },
            { 'N', "EXPRESS DOMESTIC" },
            { 'O', "EXPRESS DOMESTIC 10:30" },
            { 'P', "EXPRESS WORLDWIDE" },
            { 'Q', "MEDICAL EXPRESS" },
            { 'R', "GLOBALMAIL BUSINESS" },
            { 'S', "SAME DAY" },
            { 'T', "EXPRESS 12:00" },
            { 'U', "EXPRESS WORLDWIDE" },
            { 'V', "EUROPACK" },
            { 'W', "ECONOMY SELECT" },
            { 'X', "EXPRESS ENVELOPE" },
            { 'Y', "EXPRESS 12:00" }
        };

        public const int DefaultTimeout = 10;

        private readonly DHLProviderConfiguration _configuration;

        private const string TestServicesUrl = "http://xmlpitest-ea.dhl.com/XMLShippingServlet";
        private const string ProductionServicesUrl = "https://xmlpi-ea.dhl.com/XMLShippingServlet";

        public DHLProvider(string siteId, string password, bool useProduction) :
            this(siteId, password, useProduction, null)
        {
        }

        public DHLProvider(string siteId, string password, bool useProduction, char[] services) :
            this(siteId, password, useProduction, services, DefaultTimeout)
        {
        }

        public DHLProvider(string siteId, string password, bool useProduction, char[] services, int timeout) :
            this(new DHLProviderConfiguration(siteId, password, useProduction)
            {
                TimeOut = timeout
            }
            .IncludeServices(services))
        {
        }

        public DHLProvider(DHLProviderConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private Uri RatesUri => new Uri(_configuration.UseProduction ? ProductionServicesUrl : TestServicesUrl);

        private string BuildRatesRequestMessage(
            DateTime requestDateTime,
            DateTime pickupDateTime,
            string messageReference)
        {
            var requestCulture = CultureInfo.CreateSpecificCulture("en-US");
            var xmlSettings = new XmlWriterSettings()
            {
                Indent = true,
                Encoding = Encoding.UTF8
            };

            var isDomestic = Shipment.OriginAddress.CountryCode == Shipment.DestinationAddress.CountryCode;
            var isDutiable = !Shipment.HasDocumentsOnly && !isDomestic;

            using (var memoryStream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(memoryStream, xmlSettings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("p", "DCTRequest", "http://www.dhl.com");
                    writer.WriteStartElement("GetQuote");

                    writer.WriteStartElement("Request");
                    writer.WriteStartElement("ServiceHeader");
                    writer.WriteElementString("MessageTime", requestDateTime.ToString("O", requestCulture));
                    writer.WriteElementString("MessageReference", messageReference);
                    writer.WriteElementString("SiteID", _configuration.SiteId);
                    writer.WriteElementString("Password", _configuration.Password);
                    writer.WriteEndElement(); // </ServiceHeader>
                    writer.WriteEndElement(); // </Request>

                    WriteAddress(writer, "From", Shipment.OriginAddress);

                    writer.WriteStartElement("BkgDetails");
                    writer.WriteElementString("PaymentCountryCode", Shipment.OriginAddress.CountryCode);
                    writer.WriteElementString("Date", pickupDateTime.ToString("yyyy-MM-dd", requestCulture));
                    writer.WriteElementString("ReadyTime", $"PT{pickupDateTime:HH}H{pickupDateTime:mm}M");
                    writer.WriteElementString("ReadyTimeGMTOffset", pickupDateTime.ToString("zzz", requestCulture));
                    writer.WriteElementString("DimensionUnit", "IN");
                    writer.WriteElementString("WeightUnit", "LB");

                    writer.WriteStartElement("Pieces");
                    for (var i = 0; i < Shipment.Packages.Count; i++)
                    {
                        writer.WriteStartElement("Piece");
                        writer.WriteElementString("PieceID", $"{i + 1}");
                        writer.WriteElementString("Height", Shipment.Packages[i].RoundedHeight.ToString(requestCulture));
                        writer.WriteElementString("Depth", Shipment.Packages[i].RoundedLength.ToString(requestCulture));
                        writer.WriteElementString("Width", Shipment.Packages[i].RoundedWidth.ToString(requestCulture));
                        writer.WriteElementString("Weight", Shipment.Packages[i].RoundedWeight.ToString(requestCulture));
                        writer.WriteEndElement(); // </Piece>
                    }
                    writer.WriteEndElement(); // </Pieces>

                    if (!string.IsNullOrEmpty(_configuration.PaymentAccountNumber))
                    {
                        writer.WriteElementString("PaymentAccountNumber", _configuration.PaymentAccountNumber);
                    }
                    writer.WriteElementString("IsDutiable", isDutiable ? "Y" : "N");
                    writer.WriteElementString("NetworkTypeCode", "AL");

                    writer.WriteStartElement("QtdShp");
                    if (_configuration.ServicesIncluded.Any())
                    {
                        foreach (var serviceCode in _configuration.ServicesIncluded)
                        {
                            writer.WriteElementString("GlobalProductCode", serviceCode.ToString());
                        }
                    }
                    if (Shipment.Options.SaturdayDelivery)
                    {
                        writer.WriteStartElement("QtdShpExChrg");
                        writer.WriteElementString("SpecialServiceType", isDomestic ? "AG" : "AA");
                        writer.WriteEndElement(); // </QtdShpExChrg>
                    }
                    writer.WriteEndElement(); // </QtdShp>

                    var totalInsurance = Shipment.Packages.Sum(p => p.InsuredValue);
                    if (totalInsurance > 0)
                    {
                        writer.WriteElementString("InsuredValue", $"{totalInsurance:N}");
                        writer.WriteElementString("InsuredCurrency", "USD");
                    }

                    writer.WriteEndElement(); // </BkgDetails>

                    WriteAddress(writer, "To", Shipment.DestinationAddress);

                    if (isDutiable)
                    {
                        writer.WriteStartElement("Dutiable");
                        writer.WriteElementString("DeclaredCurrency", "USD");
                        writer.WriteElementString("DeclaredValue", $"{totalInsurance:N}");
                        writer.WriteEndElement(); // </Dutiable>
                    }

                    writer.WriteEndElement(); // </GetQuote>
                    writer.WriteEndElement(); // </p:DCTRequest>
                    writer.WriteEndDocument();
                    writer.Flush();
                    writer.Close();
                }
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        private static void WriteAddress(XmlWriter writer, string name, Address address)
        {
            writer.WriteStartElement(name);
            writer.WriteElementString("CountryCode",address.CountryCode);
            writer.WriteElementString("Postalcode", address.PostalCode);
            if (!string.IsNullOrWhiteSpace(address.City))
            {
                writer.WriteElementString("City", address.City);
            }
            writer.WriteEndElement(); // </From>
        }

        public override async Task GetRates()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(_configuration.TimeOut);

                var request = BuildRatesRequestMessage(
                    DateTime.Now,
                    Shipment.Options.ShippingDate ?? DateTime.Now,
                    GetMessageId());

                using (var httpContent = new StringContent(request, Encoding.UTF8, "text/xml"))
                {
                    var response = await httpClient.PostAsync(RatesUri, httpContent).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                        using (var reader = XmlReader.Create(responseBody))
                        {
                            var responseXml = XElement.Load(reader);
                            ParseRatesResponseMessage(responseXml);
                        }
                    }
                }
            }
        }

        private static string GetMessageId() => Guid.NewGuid().ToString().Replace("-", "");

        private void ParseRatesResponseMessage(XElement xDoc)
        {
            if (xDoc == null)
            {
                AddInternalError("Invalid response from DHL");
                return;
            }

            ParseRates(xDoc.XPathSelectElements("GetQuoteResponse/BkgDetails/QtdShp"));
            ParseErrors(xDoc.XPathSelectElements("GetQuoteResponse/Note/Condition"));
            ParseErrors(xDoc.XPathSelectElements("Response/Status/Condition"));
        }

        private void ParseRates(IEnumerable<XElement> rates)
        {
            var includedServices = _configuration.ServicesIncluded;
            var excludedServices = _configuration.ServicesExcluded;

            foreach (var rateNode in rates)
            {
                var serviceCode = rateNode.Element("GlobalProductCode")?.Value;
                if (string.IsNullOrEmpty(serviceCode) || !AvailableServices.ContainsKey(serviceCode[0]))
                {
                    AddInternalError($"Unknown DHL Global Product Code: {serviceCode}");
                    continue;
                }
                if ((includedServices.Any() && !includedServices.Contains(serviceCode[0])) ||
                    (excludedServices.Any() && excludedServices.Contains(serviceCode[0])))
                {
                    continue;
                }

                var name = rateNode.Element("ProductShortName")?.Value;
                var description = AvailableServices[serviceCode[0]];

                var totalCharges = Convert.ToDecimal(rateNode.Element("ShippingCharge")?.Value, CultureInfo.InvariantCulture);
                var currencyCode = rateNode.Element("CurrencyCode")?.Value;

                var deliveryDateValue = rateNode.XPathSelectElement("DeliveryDate")?.Value;
                var deliveryTimeValue = rateNode.XPathSelectElement("DeliveryTime")?.Value;

                if (!DateTime.TryParse(deliveryDateValue, out DateTime deliveryDate))
                    deliveryDate = DateTime.MaxValue;

                if (!string.IsNullOrEmpty(deliveryTimeValue) && deliveryTimeValue.Length >= 4)
                {
                    // Parse PTxxH or PTxxHyyM to time
                    var indexOfH = deliveryTimeValue.IndexOf('H');
                    if (indexOfH >= 3)
                    {
                        var hours = int.Parse(deliveryTimeValue.Substring(2, indexOfH - 2), CultureInfo.InvariantCulture);
                        var minutes = 0;

                        var indexOfM = deliveryTimeValue.IndexOf('M');
                        if (indexOfM > indexOfH)
                        {
                            minutes = int.Parse(deliveryTimeValue.Substring(indexOfH + 1, indexOfM - indexOfH - 1), CultureInfo.InvariantCulture);
                        }

                        deliveryDate = deliveryDate.Date + new TimeSpan(hours, minutes, 0);
                    }
                }

                AddRate(name, description, totalCharges, deliveryDate, new RateOptions()
                {
                    SaturdayDelivery = Shipment.Options.SaturdayDelivery && deliveryDate.DayOfWeek == DayOfWeek.Saturday
                },
                currencyCode);
            }
        }

        private void ParseErrors(IEnumerable<XElement> errors)
        {
            foreach (var errorNode in errors)
            {
                AddError(new Error()
                {
                    Number = errorNode.Element("ConditionCode")?.Value,
                    Description = errorNode.Element("ConditionData")?.Value
                });
            }
        }
    }
}
