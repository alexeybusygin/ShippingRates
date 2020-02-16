namespace ShippingRates
{
    public interface IRateAdjuster
    {
        Rate AdjustRate(Rate rate);
    }
}
