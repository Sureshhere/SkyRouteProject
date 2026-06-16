using FluentAssertions;
using SkyRoute.Infrastructure.Pricing;

namespace SkyRoute.Tests.Pricing;

public class BudgetWingsPricingStrategyTests
{
    private readonly BudgetWingsPricingStrategy _strategy = new();

    [Fact]
    public void ProviderName_ShouldBe_BudgetWings()
    {
        _strategy.ProviderName.Should().Be("BudgetWings");
    }

    [Theory]
    [InlineData(150.00, 1, 135.00)]   // 150 - 10% = 135.00
    [InlineData(150.00, 2, 270.00)]   // (150 * 0.90) * 2 = 270.00
    [InlineData(450.00, 1, 405.00)]   // 450 - 10% = 405.00
    [InlineData(100.00, 3, 270.00)]   // (100 * 0.90) * 3 = 270.00
    public void CalculatePrice_ShouldApply10PercentDiscount(decimal baseFare, int passengers, decimal expectedTotal)
    {
        var result = _strategy.CalculatePrice(baseFare, passengers);

        result.TotalPrice.Should().Be(expectedTotal);
        result.BaseFare.Should().Be(baseFare);
    }

    [Theory]
    [InlineData(10.00)]   // 10 - 10% = 9.00 → below minimum → 29.99
    [InlineData(20.00)]   // 20 - 10% = 18.00 → below minimum → 29.99
    [InlineData(29.99)]   // 29.99 - 10% = 26.99 → below minimum → 29.99
    [InlineData(33.32)]   // 33.32 - 10% = 29.99 → exactly minimum → 29.99
    public void CalculatePrice_ShouldEnforceMinimumPrice(decimal baseFare)
    {
        var result = _strategy.CalculatePrice(baseFare, 1);

        result.PricePerPassenger.Should().Be(29.99m);
    }

    [Fact]
    public void CalculatePrice_WhenDiscountedPriceAboveMinimum_ShouldNotEnforceMinimum()
    {
        // 500 - 10% = 450 — well above 29.99
        var result = _strategy.CalculatePrice(500m, 1);

        result.PricePerPassenger.Should().Be(450.00m);
    }

    [Fact]
    public void CalculatePrice_ShouldReturn_CorrectPerPassengerAndTotal()
    {
        var result = _strategy.CalculatePrice(150.00m, 3);

        result.PricePerPassenger.Should().Be(135.00m);
        result.TotalPrice.Should().Be(405.00m);
    }

    [Fact]
    public void CalculatePrice_ShouldIncludePricingRule()
    {
        var result = _strategy.CalculatePrice(100m, 1);

        result.PricingRule.Should().Contain("10%");
        result.PricingRule.Should().Contain("29.99");
    }

    [Fact]
    public void CalculatePrice_ShouldRoundTo2DecimalPlaces()
    {
        // 100.05 * 0.90 = 90.045 → rounds to 90.05
        var result = _strategy.CalculatePrice(100.05m, 1);

        result.PricePerPassenger.Should().Be(90.05m);
    }
}
