Rate Adjuster is an optional feature that allows you to bulk modify all rates received by `RateManager`. Possible use cases include providing a discount for the received rate or, on the contrary, charging extra for handling. Rate adjusters should implement the `IRateAdjuster` interface and can be added by calling the `RateManager.AddRateAdjuster` function. If multiple adjusters are provided, they are called sequentially in the order of adding.

Sample Rate Adjuster:

```CSHARP
    public class PercentageRateAdjuster : IRateAdjuster
    {
        private readonly decimal _amount;

        public PercentageRateAdjuster(decimal amount)
        {
            _amount = amount;
        }

        public Rate AdjustRate(Rate rate)
        {
            rate.TotalCharges = rate.TotalCharges * _amount;
            return rate;
        }
    }
```

Sample usage (provide 10% discount):

```CSHARP
    var rateManager = new RateManager();
    rateManager.AddRateAdjuster(new PercentageRateAdjuster(.9M));
```