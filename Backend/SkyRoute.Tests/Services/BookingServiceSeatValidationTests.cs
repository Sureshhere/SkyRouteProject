using FluentAssertions;
using Moq;
using SkyRoute.Application.Common;
using SkyRoute.Application.DTOs.Booking;
using SkyRoute.Application.Interfaces;
using SkyRoute.Application.Services;
using SkyRoute.Domain.Enums;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Pricing;

namespace SkyRoute.Tests.Services;

/// <summary>
/// Tests for the three-layer seat validation inside BookingService.CreateBookingAsync:
///   Layer A — seat must exist in the flight's cabin class seat map
///   Layer B — no two passengers in the same booking may share a seat
///   Layer C — seat must not already be Confirmed for the same flight/date (409)
/// </summary>
public class BookingServiceSeatValidationTests
{
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();
    private readonly Mock<IFlightRepository> _flightRepoMock = new();
    private readonly List<IFlightPricingStrategy> _strategies =
    [
        new GlobalAirPricingStrategy(),
        new BudgetWingsPricingStrategy()
    ];

    private BookingService CreateService() =>
        new(_bookingRepoMock.Object, _flightRepoMock.Object, _strategies);

    // Economy domestic flight — used for all seat validation tests
    private static Flight MakeEconomyFlight() => new()
    {
        Id = Guid.NewGuid(),
        FlightNumber = "GA001",
        DepartureTime = new TimeSpan(10, 0, 0),
        DurationMinutes = 120,
        CabinClass = CabinClass.Economy,
        BaseFare = 100m,
        IsActive = true,
        Airline = new Airline { Id = Guid.NewGuid(), Name = "GlobalAir", Code = "GA" },
        OriginAirport = new Airport { Code = "JFK", CountryCode = "US", Country = "United States" },
        DestinationAirport = new Airport { Code = "LAX", CountryCode = "US", Country = "United States" }
    };

    private static CreateBookingRequestDto MakeRequestWithSeats(Guid flightId, params string[] seatNumbers) => new()
    {
        FlightId = flightId,
        DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
        Passengers = seatNumbers.Select((seat, i) => new PassengerInputDto
        {
            FullName = $"Passenger {i + 1}",
            Email = $"passenger{i + 1}@test.com",
            DocumentNumber = "AB123456",  // valid National ID for domestic route
            SeatNumber = seat
        }).ToList()
    };

    // ── Layer A: seat not valid for cabin class ────────────────────────────────

    [Fact]
    public async Task CreateBookingAsync_SeatOutsideCabinRange_ThrowsValidationError()
    {
        // Arrange — Economy goes up to row 30; row 31 does not exist
        var flight = MakeEconomyFlight();
        _flightRepoMock.Setup(r => r.GetByIdAsync(flight.Id)).ReturnsAsync(flight);

        var request = MakeRequestWithSeats(flight.Id, "31A");

        // Act & Assert
        await CreateService().Invoking(s => s.CreateBookingAsync(request, Guid.NewGuid()))
            .Should().ThrowAsync<AppException>()
            .WithMessage("*not valid for cabin class*");
    }

    // ── Layer B: duplicate seat within the same booking ───────────────────────

    [Fact]
    public async Task CreateBookingAsync_TwoPassengersRequestSameSeat_ThrowsValidationError()
    {
        // Arrange — both passengers choose seat 5C
        var flight = MakeEconomyFlight();
        _flightRepoMock.Setup(r => r.GetByIdAsync(flight.Id)).ReturnsAsync(flight);

        var request = MakeRequestWithSeats(flight.Id, "5C", "5C");

        // Act & Assert
        await CreateService().Invoking(s => s.CreateBookingAsync(request, Guid.NewGuid()))
            .Should().ThrowAsync<AppException>()
            .WithMessage("*same seat*");
    }

    // ── Layer C: seat already taken by a Confirmed booking ────────────────────

    [Fact]
    public async Task CreateBookingAsync_SeatAlreadyConfirmedOnFlightDate_Throws409()
    {
        // Arrange — seat 10A is already occupied by an existing Confirmed booking
        var flight = MakeEconomyFlight();
        _flightRepoMock.Setup(r => r.GetByIdAsync(flight.Id)).ReturnsAsync(flight);
        _bookingRepoMock.Setup(r => r.GetOccupiedSeatsAsync(flight.Id, It.IsAny<DateOnly>()))
            .ReturnsAsync(new List<string> { "10A" });

        var request = MakeRequestWithSeats(flight.Id, "10A");

        // Act & Assert
        var ex = await CreateService().Invoking(s => s.CreateBookingAsync(request, Guid.NewGuid()))
            .Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(409);
    }

    // ── Empty seat number (pre-Layer A) ───────────────────────────────────────
    // FluentValidation's NotEmpty rule would catch "" at the controller boundary.
    // At the service level Layer A also rejects it because "" is not in any cabin's
    // seat list, so the service is safe even without prior validation.

    [Fact]
    public async Task CreateBookingAsync_EmptySeatNumber_ThrowsValidationError()
    {
        // Arrange
        var flight = MakeEconomyFlight();
        _flightRepoMock.Setup(r => r.GetByIdAsync(flight.Id)).ReturnsAsync(flight);

        var request = MakeRequestWithSeats(flight.Id, ""); // empty seat number

        // Act & Assert — Layer A rejects it: "" is not in Economy's seat map
        await CreateService().Invoking(s => s.CreateBookingAsync(request, Guid.NewGuid()))
            .Should().ThrowAsync<AppException>();
    }

    // ── Happy path: all three layers pass ─────────────────────────────────────

    [Fact]
    public async Task CreateBookingAsync_WithValidUniqueAvailableSeats_PersistsAllSeatNumbers()
    {
        // Arrange
        var flight = MakeEconomyFlight();
        _flightRepoMock.Setup(r => r.GetByIdAsync(flight.Id)).ReturnsAsync(flight);
        _bookingRepoMock.Setup(r => r.GetOccupiedSeatsAsync(flight.Id, It.IsAny<DateOnly>()))
            .ReturnsAsync(new List<string>());

        Booking? capturedBooking = null;
        _bookingRepoMock.Setup(r => r.AddAsync(It.IsAny<Booking>()))
            .Callback<Booking>(b => capturedBooking = b)
            .ReturnsAsync((Booking b) => b);

        var request = MakeRequestWithSeats(flight.Id, "5A", "5B");

        // Act
        await CreateService().CreateBookingAsync(request, Guid.NewGuid());

        // Assert — seat numbers are stored against the correct passengers
        capturedBooking.Should().NotBeNull();
        capturedBooking!.PassengerDetails.Should().Contain(p => p.SeatNumber == "5A");
        capturedBooking.PassengerDetails.Should().Contain(p => p.SeatNumber == "5B");
    }
}
