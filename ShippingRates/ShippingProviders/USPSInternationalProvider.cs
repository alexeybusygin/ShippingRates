using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using ShippingRates.Models;

namespace ShippingRates.ShippingProviders
{
    /// <summary>
    /// USPS International Rates Provider
    /// </summary>
    public class USPSInternationalProvider : USPSBaseProvider
    {
        private readonly Dictionary<string, string> _serviceCodes = new Dictionary<string, string>
        {
            {"Priority Mail Express International","Priority Mail Express International"},
            {"Priority Mail International","Priority Mail International"},
            {"Global Express Guaranteed (GXG)","Global Express Guaranteed (GXG)"},
            {"Global Express Guaranteed Document","Global Express Guaranteed Document"},
            {"Global Express Guaranteed Non-Document Rectangular","Global Express Guaranteed Non-Document Rectangular"},
            {"Global Express Guaranteed Non-Document Non-Rectangular","Global Express Guaranteed Non-Document Non-Rectangular"},
            {"Priority Mail International Flat Rate Envelope","Priority Mail International Flat Rate Envelope"},
            {"Priority Mail International Medium Flat Rate Box","Priority Mail International Medium Flat Rate Box"},
            {"Priority Mail Express International Flat Rate Envelope","Priority Mail Express International Flat Rate Envelope"},
            {"Priority Mail International Large Flat Rate Box","Priority Mail International Large Flat Rate Box"},
            {"USPS GXG Envelopes","USPS GXG Envelopes"},
            {"First-Class Mail International Letter","First-Class Mail International Letter"},
            {"First-Class Mail International Large Envelope","First-Class Mail International Large Envelope"},
            {"First-Class Package International Service","First-Class Package International Service"},
            {"Priority Mail International Small Flat Rate Box","Priority Mail International Small Flat Rate Box"},
            {"Priority Mail Express International Legal Flat Rate Envelope","Priority Mail Express International Legal Flat Rate Envelope"},
            {"Priority Mail International Gift Card Flat Rate Envelope","Priority Mail International Gift Card Flat Rate Envelope"},
            {"Priority Mail International Window Flat Rate Envelope","Priority Mail International Window Flat Rate Envelope"},
            {"Priority Mail International Small Flat Rate Envelope","Priority Mail International Small Flat Rate Envelope"},
            {"First-Class Mail International Postcard","First-Class Mail International Postcard"},
            {"Priority Mail International Legal Flat Rate Envelope","Priority Mail International Legal Flat Rate Envelope"},
            {"Priority Mail International Padded Flat Rate Envelope","Priority Mail International Padded Flat Rate Envelope"},
            {"Priority Mail International DVD Flat Rate priced box","Priority Mail International DVD Flat Rate priced box"},
            {"Priority Mail International Large Video Flat Rate priced box","Priority Mail International Large Video Flat Rate priced box"},
            {"Priority Mail Express International Flat Rate Boxes","Priority Mail Express International Flat Rate Boxes"},
            {"Priority Mail Express International Padded Flat Rate Envelope","Priority Mail Express International Padded Flat Rate Envelope"}
        };

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        public USPSInternationalProvider(string userId, string service = USPS.Services.All) :
            this(new USPSProviderConfiguration(userId) { Service = service })
        {
        }

        public USPSInternationalProvider(USPSProviderConfiguration configuration)
            : base(configuration) { }

        public USPSInternationalProvider(USPSProviderConfiguration configuration, HttpClient httpClient)
            : base(configuration, httpClient) { }

        public bool Commercial { get; set; }

        /// <summary>
        /// Returns the supported service codes
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetServiceCodes()
        {
            if (_serviceCodes != null && _serviceCodes.Count > 0)
            {
                var serviceCodes = new Dictionary<string, string>();

                foreach (var serviceCodeKey in _serviceCodes.Keys)
                {
                    var serviceCode = _serviceCodes[serviceCodeKey];
                    serviceCodes.Add(serviceCodeKey, serviceCode);
                }

                return serviceCodes;
            }

            return null;
        }

        public override async Task<RateResult> GetRatesAsync(Shipment shipment)
        {
            var httpClient = IsExternalHttpClient ? HttpClient : new HttpClient();
            var resultBuilder = new RateResultBuilder(Name);

            try
            {
                var requestXmlString = GetRequestXmlString(shipment);
                var rateUri = new Uri($"{ProductionUrl}?API=IntlRateV2&XML={requestXmlString}");
                var response = await httpClient.GetStringAsync(rateUri).ConfigureAwait(false);

                ParseResult(shipment, response, resultBuilder);
            }
            catch (Exception e)
            {
                resultBuilder.AddInternalError($"USPS International Provider Exception: {e.Message}");
            }
            finally
            {
                if (!IsExternalHttpClient && httpClient != null)
                    httpClient.Dispose();
            }

            return resultBuilder.GetRateResult();
        }

        private string GetRequestXmlString(Shipment shipment)
        {
            var sb = new StringBuilder();

            var settings = new XmlWriterSettings
            {
                Indent = false,
                OmitXmlDeclaration = true,
                NewLineHandling = NewLineHandling.None
            };

            using (var writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartElement("IntlRateV2Request");
                writer.WriteAttributeString("USERID", _configuration.UserId);

                writer.WriteElementString("Revision", "2");
                var i = 0;
                foreach (var package in shipment.Packages)
                {
                    //<Package ID="2ND">
                    //  <Pounds>0</Pounds>
                    //  <Ounces>3</Ounces>
                    //  <MailType>Envelope</MailType>
                    //  <ValueOfContents>750</ValueOfContents>
                    //  <Country>Algeria</Country>
                    //  <Container></Container>
                    //  <Width></Width>
                    //  <Length></Length>
                    //  <Height></Height>
                    //  <Girth></Girth>
                    //  <CommercialFlag>N</CommercialFlag>
                    //</Package>

                    writer.WriteStartElement("Package");
                    writer.WriteAttributeString("ID", i.ToString());
                    writer.WriteElementString("Pounds", package.PoundsAndOunces.Pounds.ToString());
                    writer.WriteElementString("Ounces", package.PoundsAndOunces.Ounces.ToString());
                    writer.WriteElementString("MailType", "All");
                    writer.WriteElementString("ValueOfContents", package.InsuredValue.ToString());
                    writer.WriteElementString("Country", shipment.DestinationAddress.GetCountryName());
                    writer.WriteElementString("Container", string.IsNullOrEmpty(package.Container) ? "RECTANGULAR" : package.Container);
                    writer.WriteElementString("Width", package.GetRoundedWidth(UnitsSystem.USCustomary).ToString());
                    writer.WriteElementString("Length", package.GetRoundedLength(UnitsSystem.USCustomary).ToString());
                    writer.WriteElementString("Height", package.GetRoundedHeight(UnitsSystem.USCustomary).ToString());
                    writer.WriteElementString("Girth", package.GetCalculatedGirth(UnitsSystem.USCustomary).ToString());
                    writer.WriteElementString("OriginZip", shipment.OriginAddress.PostalCode);
                    writer.WriteElementString("CommercialFlag", Commercial ? "Y" : "N");

                    // ContentType must be set to Documents to get First-Class International Mail rates
                    if (package is DocumentsPackage)
                    {
                        writer.WriteStartElement("Content");
                        writer.WriteElementString("ContentType", "Documents");
                        writer.WriteEndElement();
                    }

                    //TODO: Figure out DIM Weights
                    //writer.WriteElementString("Size", package.IsOversize ? "LARGE" : "REGULAR");
                    //writer.WriteElementString("Length", package.RoundedLength.ToString());
                    //writer.WriteElementString("Width", package.RoundedWidth.ToString());
                    //writer.WriteElementString("Height", package.RoundedHeight.ToString());
                    //writer.WriteElementString("Girth", package.CalculatedGirth.ToString());
                    i++;
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.Flush();
            }

            return sb.ToString();
        }

        public static bool IsDomesticUSPSAvailable(Shipment shipment)
        {
            return shipment.OriginAddress.IsUnitedStatesAddress() && shipment.DestinationAddress.IsUnitedStatesAddress();
        }

        private void ParseResult(Shipment shipment, string response, RateResultBuilder resultBuilder)
        {
            var document = XDocument.Load(new StringReader(response));

            var rates = document.Descendants("Service").GroupBy(item => (string) item.Element("SvcDescription")).Select(g => new {Name = g.Key, TotalCharges = g.Sum(x => Decimal.Parse((string) x.Element("Postage")))});

            foreach (var r in rates)
            {
                var name = Regex.Replace(r.Name, "&lt.*gt;", "");

                if (_configuration.Service == name || _configuration.Service == "ALL")
                {
                    resultBuilder.AddRate(name, string.Concat("USPS ", name), r.TotalCharges, DateTime.Now.AddDays(30), null, USPSCurrencyCode);
                }
            }

            //check for errors
            ParseErrors(document.Root, resultBuilder);
        }
    }
}
