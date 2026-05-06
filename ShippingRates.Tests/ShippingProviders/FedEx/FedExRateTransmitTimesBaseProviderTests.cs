using Newtonsoft.Json.Linq;
using ShippingRates.Models;
using ShippingRates.OpenApi.FedEx.RateTransitTimes;
using ShippingRates.ShippingProviders.FedEx;

namespace ShippingRates.Tests.ShippingProviders.FedEx;

public class FedExRateTransmitTimesBaseProviderTests
{
    private readonly TestFedExProvider _provider = new();

    [Test]
    public void CreateRateRequest_RequestsTransitTimesWithoutSaturdayDeliveryByDefault()
    {
        var request = _provider.CreateRequest(CreateShipment());

        var controlParameters = JObject.FromObject(request.RateRequestControlParameters);

        using (Assert.EnterMultipleScope())
        {
            Assert.That((bool?)controlParameters["returnTransitTimes"], Is.True);
            Assert.That(controlParameters.ContainsKey("variableOptions"), Is.False);
            Assert.That(controlParameters.ContainsKey("servicesNeededOnRateFailure"), Is.False);
            Assert.That(controlParameters.ContainsKey("rateSortOrder"), Is.False);
        }
    }

    [Test]
    public void CreateRateRequest_SetsSaturdayDeliveryVariableOptionWhenRequested()
    {
        var request = _provider.CreateRequest(CreateShipment(new ShipmentOptions
        {
            SaturdayDelivery = true
        }));

        var controlParameters = JObject.FromObject(request.RateRequestControlParameters);

        using (Assert.EnterMultipleScope())
        {
            Assert.That((bool?)controlParameters["returnTransitTimes"], Is.True);
            Assert.That((string?)controlParameters["variableOptions"], Is.EqualTo("SATURDAY_DELIVERY"));
        }
    }

    [Test]
    public void ProcessReply_UsesCommitDateWhenDeliveryDateIsMissing()
    {
        var shipment = CreateShipment();
        var reply = CreateReply(new OperationalDetail
        {
            CommitDate = "2026-04-27T17:00:00"
        });

        var result = _provider.ProcessReply(shipment, reply);

        Assert.That(result.Rates.Single().GuaranteedDelivery, Is.EqualTo(new DateTime(2026, 4, 27, 17, 0, 0)));
    }

    [Test]
    public void ProcessReply_UsesCommitDateDetailWhenOperationalDatesAreMissing()
    {
        var shipment = CreateShipment();
        var reply = CreateReply(
            new OperationalDetail(),
            new Commit
            {
                DateDetail = new DateDetail
                {
                    DayFormat = "2026-04-28T10:30:00"
                }
            });

        var result = _provider.ProcessReply(shipment, reply);

        Assert.That(result.Rates.Single().GuaranteedDelivery, Is.EqualTo(new DateTime(2026, 4, 28, 10, 30, 0)));
    }

    private static Shipment CreateShipment(ShipmentOptions? options = null)
    {
        return new Shipment(
            new Address("Milford", "PA", "18337", "US"),
            new Address("Houston", "TX", "77023", "US"),
            [new Package(1, 1, 1, 1, 0)],
            options);
    }

    private static BaseProcessOutputVO CreateReply(OperationalDetail operationalDetail, Commit? commit = null)
    {
        return new BaseProcessOutputVO
        {
            RateReplyDetails =
            [
                new RateReplyDetail
                {
                    ServiceType = "FEDEX_GROUND",
                    OperationalDetail = operationalDetail,
                    Commit = commit,
                    RatedShipmentDetails =
                    [
                        new RatedShipmentDetail
                        {
                            RateType = RatedShipmentDetailRateType.LIST,
                            TotalNetCharge = 12.34,
                            ShipmentRateDetail = new ShipmentRateDetail
                            {
                                Currency = "USD"
                            }
                        }
                    ]
                }
            ]
        };
    }

    private sealed class TestFedExProvider : FedExRateTransmitTimesBaseProvider<TestFedExProvider>
    {
        public TestFedExProvider()
            : base(new FedExProviderConfiguration
            {
                ClientId = "client-id",
                ClientSecret = "client-secret",
                AccountNumber = "account-number"
            })
        {
        }

        public override string Name => "FedEx";

        protected override Dictionary<string, string> ServiceCodes => new()
        {
            ["FEDEX_GROUND"] = "FedEx Ground"
        };

        public Full_Schema_Quote_Rate CreateRequest(Shipment shipment)
        {
            return CreateRateRequest(shipment);
        }

        public RateResult ProcessReply(Shipment shipment, BaseProcessOutputVO reply)
        {
            var aggregator = new RateResultAggregator(Name);
            base.ProcessReply(aggregator, shipment, reply);
            return aggregator.Build();
        }

        protected override void SetShipmentDetails(Full_Schema_Quote_Rate request)
        {
        }
    }
}
