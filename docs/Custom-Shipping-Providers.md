# Custom shipping providers

ShippingRates can be extended with your own provider by implementing `IShippingProvider` or, more commonly, by inheriting from `AbstractShippingProvider`.

This is useful when you need to combine carrier APIs with business-specific delivery methods such as:

- Local courier service
- Warehouse transfer pricing
- Flat-rate regional delivery
- Marketplace-specific shipping rules

## When to build a custom provider

Create a custom provider when the delivery method should participate in the normal `RateManager` flow and return rates alongside UPS, FedEx, USPS, or DHL.

If you only need to adjust existing carrier prices, prefer [Rate adjusters](Rate-Adjusters.md).

## Implementation pattern

A custom provider typically needs to:

1. Inherit from `AbstractShippingProvider`
2. Set a provider `Name`
3. Inspect the incoming `Shipment`
4. Return a `RateResult` containing zero or more `Rate` instances
5. Add provider-facing errors to `Errors` or diagnostics to `InternalErrors` when needed

## Example: Local Van Delivery

The example below adds a `LocalVanDeliveryProvider` that:

- Returns a flat `$24.95` rate
- Guarantees next-business-day delivery
- Serves only a small ZIP-code area around Allex, Texas
- Ignores shipments outside that local delivery area

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShippingRates;
using ShippingRates.ShippingProviders;

public sealed class LocalVanDeliveryProvider : AbstractShippingProvider
{
    private static readonly HashSet<string> SupportedOriginZipCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "78601",
        "78602",
        "78621"
    };

    private static readonly HashSet<string> SupportedDestinationZipCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "78601",
        "78602",
        "78621",
        "78653",
        "78724"
    };

    private const decimal FlatRate = 24.95m;
    private const string CurrencyCode = "USD";

    public override string Name => "Local Van Delivery";

    public override Task<RateResult> GetRatesAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = new RateResult();

        if (!CanServe(shipment))
        {
            return Task.FromResult(result);
        }

        var deliveryDate = GetNextBusinessDay(shipment.Options.ShippingDate ?? DateTime.Today);

        result.Rates.Add(new Rate(
            provider: Name,
            providerCode: "LOCAL_VAN_NEXT_DAY",
            name: "Local Van Delivery Next Day",
            totalCharges: FlatRate,
            delivery: deliveryDate,
            options: new RateOptions
            {
                SaturdayDelivery = false
            },
            currencyCode: CurrencyCode));

        return Task.FromResult(result);
    }

    private static bool CanServe(Shipment shipment)
    {
        if (!shipment.OriginAddress.IsUnitedStatesAddress() || !shipment.DestinationAddress.IsUnitedStatesAddress())
        {
            return false;
        }

        var originZip = NormalizeZipCode(shipment.OriginAddress.PostalCode);
        var destinationZip = NormalizeZipCode(shipment.DestinationAddress.PostalCode);

        if (originZip == null || destinationZip == null)
        {
            return false;
        }

        return SupportedOriginZipCodes.Contains(originZip)
            && SupportedDestinationZipCodes.Contains(destinationZip);
    }

    private static string? NormalizeZipCode(string? postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
        {
            return null;
        }

        var digits = new string(postalCode.Where(char.IsDigit).ToArray());
        return digits.Length >= 5 ? digits[..5] : null;
    }

    private static DateTime GetNextBusinessDay(DateTime shippingDate)
    {
        var date = shippingDate.Date.AddDays(1);

        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            date = date.AddDays(1);
        }

        // Use an end-of-day timestamp so the example reads like a real local delivery promise.
        return date.AddHours(17);
    }
}
```

## Usage

Add the custom provider the same way as built-in providers:

```csharp
var rateManager = new RateManager();

rateManager.AddProvider(new UPSProvider(upsConfiguration, httpClient));
rateManager.AddProvider(new LocalVanDeliveryProvider());

var shipment = await rateManager.GetRatesAsync(origin, destination, packages);
```

If the shipment matches the supported ZIP-code area, `shipment.Rates` will include the local van rate next to carrier rates.

## Result design guidance

When returning custom rates:

- Use a stable `providerCode` such as `LOCAL_VAN_NEXT_DAY`
- Keep `name` user-friendly because many applications display it directly
- Set `CurrencyCode` explicitly
- Return an empty result when the provider is not applicable
- Reserve `Errors` for user-facing business errors
- Reserve `InternalErrors` for diagnostics and implementation failures

## Notes

- `AbstractShippingProvider` is the simplest base class when your provider does not need special HTTP behavior.
- If your custom provider calls an external API, you can still inherit from `AbstractShippingProvider` and use its `HttpClient` management helpers.
- Custom providers can be combined with built-in providers and with [Rate adjusters](Rate-Adjusters.md).
