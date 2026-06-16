using SkyRoute.Application.Common;
using SkyRoute.Application.Interfaces;

namespace SkyRoute.Infrastructure.Pricing;

/// <summary>
/// BudgetWings pricing: Base fare - 10% promotional discount.
/// Minimum price per passenger is $29.99. Discount applied to base fare only.
/// </summary>
public class BudgetWingsPricingStrategy : IFlightPricingStrategy
{
    private const decimal MinimumPrice = 29.99m;

    public string ProviderName => "BudgetWings";

    public PricingResult CalculatePrice(decimal baseFare, int numberOfPassengers)
    {
        var discounted = baseFare - (baseFare * 0.10m);
        var pricePerPassenger = Math.Round(Math.Max(discounted, MinimumPrice), 2, MidpointRounding.AwayFromZero);
        var totalPrice = Math.Round(pricePerPassenger * numberOfPassengers, 2, MidpointRounding.AwayFromZero);

        return new PricingResult
        {
            BaseFare = baseFare,
            PricePerPassenger = pricePerPassenger,
            TotalPrice = totalPrice,
            PricingRule = $"Base fare - 10% discount (minimum ${MinimumPrice})"
        };
    }
}
