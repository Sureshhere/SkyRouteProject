using FluentAssertions;
using SkyRoute.Application.Utilities;

namespace SkyRoute.Tests.Utilities;

public class SeatConfigurationTests
{
    // ── Seat counts ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Economy",    180)]   // 30 rows × 6 columns (ABCDEF)
    [InlineData("Business",    40)]   // 10 rows × 4 columns (ABCD)
    [InlineData("FirstClass",  10)]   //  5 rows × 2 columns (AB)
    public void GetSeatsForCabin_ReturnsCorrectSeatCount(string cabinClass, int expectedCount)
    {
        // Arrange & Act
        var seats = SeatConfiguration.GetSeatsForCabin(cabinClass);

        // Assert
        seats.Count.Should().Be(expectedCount);
    }

    // ── First seat is always 1A ────────────────────────────────────────────────

    [Theory]
    [InlineData("Economy")]
    [InlineData("Business")]
    [InlineData("FirstClass")]
    public void GetSeatsForCabin_FirstSeatIs1A(string cabinClass)
    {
        // Arrange & Act
        var seats = SeatConfiguration.GetSeatsForCabin(cabinClass);

        // Assert
        seats.First().Should().Be("1A");
    }

    // ── Last seat matches cabin boundary ──────────────────────────────────────

    [Theory]
    [InlineData("Economy",    "30F")]
    [InlineData("Business",   "10D")]
    [InlineData("FirstClass",  "5B")]
    public void GetSeatsForCabin_LastSeatMatchesCabinBoundary(string cabinClass, string expectedLast)
    {
        // Arrange & Act
        var seats = SeatConfiguration.GetSeatsForCabin(cabinClass);

        // Assert
        seats.Last().Should().Be(expectedLast);
    }

    // ── Unknown cabin class throws ─────────────────────────────────────────────

    [Fact]
    public void GetSeatsForCabin_UnknownCabinClass_ThrowsArgumentException()
    {
        // Arrange
        var act = () => SeatConfiguration.GetSeatsForCabin("PremiumEconomy");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*PremiumEconomy*");
    }
}
