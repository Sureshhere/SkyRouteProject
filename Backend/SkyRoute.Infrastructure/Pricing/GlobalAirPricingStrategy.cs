using SkyRoute.Application.Common;
using SkyRoute.Application.Interfaces;

namespace SkyRoute.Infrastructure.Pricing;

/// <summary>
/// GlobalAir pricing: Base fare + 15% fuel surcharge, rounded to 2 decimal places.
/// </summary>
public class GlobalAirPricingStrategy : IFlightPricingStrategy
{
    public string ProviderName => "GlobalAir";

    public PricingResult CalculatePrice(decimal baseFare, int numberOfPassengers)
    {
        var pricePerPassenger = Math.Round(baseFare * 1.15m, 2, MidpointRounding.AwayFromZero);
        var totalPrice = Math.Round(pricePerPassenger * numberOfPassengers, 2, MidpointRounding.AwayFromZero);

        return new PricingResult
        {
            BaseFare = baseFare,
            PricePerPassenger = pricePerPassenger,
            TotalPrice = totalPrice,
            PricingRule = "Base fare + 15% fuel surcharge"
        };
    }
}
