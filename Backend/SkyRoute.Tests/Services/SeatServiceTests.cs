using FluentAssertions;
using Moq;
using SkyRoute.Application.Common;
using SkyRoute.Application.Interfaces;
using SkyRoute.Application.Services;
using SkyRoute.Domain.Enums;
using SkyRoute.Domain.Models;

namespace SkyRoute.Tests.Services;

public class SeatServiceTests
{
    private readonly Mock<IFlightRepository> _flightRepoMock = new();
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();

    private SeatService CreateService() =>
        new(_flightRepoMock.Object, _bookingRepoMock.Object);

    private static Flight MakeEconomyFlight() => new()
    {
        Id = Guid.NewGuid(),
        FlightNumber = "GA001",
        CabinClass = CabinClass.Economy,
        BaseFare = 100m,
        DepartureTime = new TimeSpan(10, 0, 0),
        DurationMinutes = 120,
        IsActive = true,
        Airline = new Airline { Id = Guid.NewGuid(), Name = "GlobalAir", Code = "GA" },
        OriginAirport = new Airport { Code = "JFK", CountryCode = "US", Country = "United States" },
        DestinationAirport = new Airport { Code = "LAX", CountryCode = "US", Country = "United States" }
    };

    // ── Available = All − Occupied ─────────────────────────────────────────────

    [Fact]
    public async Task GetAvailableSeatsAsync_SubtractsOccupiedSeatsFromCabinTotal()
    {
        // Arrange
        var flight = MakeEconomyFlight();
        var departureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var occupiedSeats = new List<string> { "1A", "2B", "3C" };

        _flightRepoMock.Setup(r => r.GetByIdAsync(flight.Id)).ReturnsAsync(flight);
        _bookingRepoMock.Setup(r => r.GetOccupiedSeatsAsync(flight.Id, departureDate))
            .ReturnsAsync(occupiedSeats);

        // Act
        var result = await CreateService().GetAvailableSeatsAsync(flight.Id, departureDate);

        // Assert
        result.AvailableSeats.Should().HaveCount(177); // 180 − 3
        result.AvailableSeats.Should().NotContain("1A");
        result.AvailableSeats.Should().NotContain("2B");
        result.AvailableSeats.Should().NotContain("3C");
    }

    // ── Cancelled bookings do not reduce availability ──────────────────────────
    // The repository's GetOccupiedSeatsAsync only returns seats from Confirmed
    // bookings. When all bookings are cancelled, it returns an empty list, and
    // every cabin seat must appear as available.

    [Fact]
    public async Task GetAvailableSeatsAsync_WhenAllBookingsAreCancelled_AllCabinSeatsAvailable()
    {
        // Arrange
        var flight = MakeEconomyFlight();
        var departureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        _flightRepoMock.Setup(r => r.GetByIdAsync(flight.Id)).ReturnsAsync(flight);
        _bookingRepoMock.Setup(r => r.GetOccupiedSeatsAsync(flight.Id, departureDate))
            .ReturnsAsync(new List<string>()); // repo returns empty — only Confirmed counts

        // Act
        var result = await CreateService().GetAvailableSeatsAsync(flight.Id, departureDate);

        // Assert — all 180 Economy seats are available
        result.AvailableSeats.Should().HaveCount(180);
    }

    // ── Flight not found → 404 ────────────────────────────────────────────────

    [Fact]
    public async Task GetAvailableSeatsAsync_WhenFlightNotFound_ThrowsAppException404()
    {
        // Arrange
        _flightRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Flight?)null);

        // Act
        var act = () => CreateService().GetAvailableSeatsAsync(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow));

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(404);
    }

    // ── DTO fields populated correctly ────────────────────────────────────────

    [Fact]
    public async Task GetAvailableSeatsAsync_ReturnsCorrectFlightIdAndCabinClass()
    {
        // Arrange
        var flight = MakeEconomyFlight();
        var departureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        _flightRepoMock.Setup(r => r.GetByIdAsync(flight.Id)).ReturnsAsync(flight);
        _bookingRepoMock.Setup(r => r.GetOccupiedSeatsAsync(flight.Id, departureDate))
            .ReturnsAsync(new List<string>());

        // Act
        var result = await CreateService().GetAvailableSeatsAsync(flight.Id, departureDate);

        // Assert
        result.FlightId.Should().Be(flight.Id);
        result.CabinClass.Should().Be("Economy");
    }
}
