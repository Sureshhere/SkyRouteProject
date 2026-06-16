using FluentAssertions;
using SkyRoute.Infrastructure.Pricing;

namespace SkyRoute.Tests.Pricing;

public class GlobalAirPricingStrategyTests
{
    private readonly GlobalAirPricingStrategy _strategy = new();

    [Fact]
    public void ProviderName_ShouldBe_GlobalAir()
    {
        _strategy.ProviderName.Should().Be("GlobalAir");
    }

    [Theory]
    [InlineData(100.00, 1, 115.00)]   // 100 + 15% = 115.00
    [InlineData(150.00, 2, 345.00)]   // (150 * 1.15) * 2 = 345.00
    [InlineData(200.00, 3, 690.00)]   // (200 * 1.15) * 3 = 690.00
    [InlineData(450.00, 1, 517.50)]   // 450 + 15% = 517.50
    [InlineData(900.00, 1, 1035.00)]  // 900 + 15% = 1035.00
    public void CalculatePrice_ShouldApply15PercentSurcharge(decimal baseFare, int passengers, decimal expectedTotal)
    {
        var result = _strategy.CalculatePrice(baseFare, passengers);

        result.TotalPrice.Should().Be(expectedTotal);
        result.BaseFare.Should().Be(baseFare);
    }

    [Fact]
    public void CalculatePrice_ShouldReturn_CorrectPerPassengerPrice()
    {
        var result = _strategy.CalculatePrice(150.00m, 2);

        result.PricePerPassenger.Should().Be(172.50m); // 150 * 1.15 = 172.50
        result.TotalPrice.Should().Be(345.00m);
    }

    [Fact]
    public void CalculatePrice_ShouldRoundTo2DecimalPlaces()
    {
        // 100.01 * 1.15 = 115.0115 → rounds to 115.01
        var result = _strategy.CalculatePrice(100.01m, 1);

        result.PricePerPassenger.Should().Be(115.01m);
    }

    [Fact]
    public void CalculatePrice_ShouldIncludePricingRule()
    {
        var result = _strategy.CalculatePrice(100m, 1);

        result.PricingRule.Should().Contain("15%");
    }
}
