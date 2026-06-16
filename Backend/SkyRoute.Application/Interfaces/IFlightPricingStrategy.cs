using SkyRoute.Domain.Models;
using SkyRoute.Application.Common;

namespace SkyRoute.Application.Interfaces;

public interface IFlightPricingStrategy
{
    string ProviderName { get; }
    PricingResult CalculatePrice(decimal baseFare, int numberOfPassengers);
}
