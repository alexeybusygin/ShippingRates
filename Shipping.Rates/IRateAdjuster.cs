namespace Shipping.Rates
{
    public interface IRateAdjuster
    {
        Rate AdjustRate(Rate rate);
    }
}
