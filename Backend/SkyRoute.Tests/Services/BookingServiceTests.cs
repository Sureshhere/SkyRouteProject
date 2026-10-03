using FluentAssertions;
using Moq;
using SkyRoute.Application.Common;
using SkyRoute.Application.DTOs.Booking;
using SkyRoute.Application.Interfaces;
using SkyRoute.Application.Services;
using SkyRoute.Application.Validators;
using SkyRoute.Domain.Enums;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Pricing;

namespace SkyRoute.Tests.Services;

public class BookingServiceTests
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

    private static Flight MakeFlight(string airlineName, string originCountry, string destCountry) => new()
    {
        Id = Guid.NewGuid(),
        FlightNumber = "GA001",
        DepartureTime = new TimeSpan(10, 0, 0),
        DurationMinutes = 330,
        CabinClass = CabinClass.Economy,
        BaseFare = 150m,
        IsActive = true,
        Airline = new Airline { Id = Guid.NewGuid(), Name = airlineName, Code = "GA" },
        OriginAirport = new Airport { Code = "JFK", CountryCode = originCountry, Country = "United States" },
        DestinationAirport = new Airport { Code = "LAX", CountryCode = destCountry, Country = "United States" }
    };

    private static CreateBookingRequestDto MakeRequest(Guid flightId, string documentNumber, int passengerCount = 1) => new()
    {
        FlightId = flightId,
        DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
        Passengers = Enumerable.Range(1, passengerCount)
            .Select(i => new PassengerInputDto
            {
                FullName = $"Passenger {i}",
                Email = $"passenger{i}@test.com",
                DocumentNumber = documentNumber,
                SeatNumber = $"{i}A"
            }).ToList()
    };

    [Fact]
    public async Task CreateBookingAsync_DomesticFlight_WithValidNationalId_ShouldSucceed()
    {
        var flight = MakeFlight("GlobalAir", "US", "US"); // domestic
        _flightRepoMock.Setup(r => r.GetByIdAsync(flight.Id)).ReturnsAsync(flight);
        _bookingRepoMock.Setup(r => r.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b) => b);
        _bookingRepoMock.Setup(r => r.GetOccupiedSeatsAsync(flight.Id, It.IsAny<DateOnly>())).ReturnsAsync(new List<string>());

        var request = MakeRequest(flight.Id, "AB123456"); // valid National ID
        var service = CreateService();

        var result = await service.CreateBookingAsync(request, Guid.NewGuid());

        result.Should().NotBeNull();
        result.BookingReferenceCode.Should().StartWith("SK");
        result.Passengers.First().DocumentType.Should().Be("NationalId");
    }

    [Fact]
    public async Task CreateBookingAsync_InternationalFlight_WithValidPassport_ShouldSucceed()
    {
        var flight = MakeFlight("GlobalAir", "US", "GB"); // international
        _flightRepoMock.Setup(r => r.GetByIdAsync(flight.Id)).ReturnsAsync(flight);
        _bookingRepoMock.Setup(r => r.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b) => b);
        _bookingRepoMock.Setup(r => r.GetOccupiedSeatsAsync(flight.Id, It.IsAny<DateOnly>())).ReturnsAsync(new List<string>());

        var request = MakeRequest(flight.Id, "A1234567"); // valid Passport
        var service = CreateService();

        var result = await service.CreateBookingAsync(request, Guid.NewGuid());

        result.Passengers.First().DocumentType.Should().Be("PassportNumber");
    }

    [Fact]
    public async Task CreateBookingAsync_DomesticFlight_WithPassportNumber_ShouldThrow()
    {
        var flight = MakeFlight("GlobalAir", "US", "US"); // domestic — expects NationalId
        _flightRepoMock.Setup(r => r.GetByIdAsync(flight.Id)).ReturnsAsync(flight);

        var request = MakeRequest(flight.Id, "A123456"); // passport format on domestic route
        var service = CreateService();

        await service.Invoking(s => s.CreateBookingAsync(request, Guid.NewGuid()))
            .Should().ThrowAsync<AppException>()
            .WithMessage("*National ID*");
    }

    [Fact]
    public async Task CreateBookingAsync_WhenFlightNotFound_ShouldThrow404()
    {
        _flightRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Flight?)null);

        var request = MakeRequest(Guid.NewGuid(), "AB123456");
        var service = CreateService();

        var ex = await service.Invoking(s => s.CreateBookingAsync(request, Guid.NewGuid()))
            .Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateBookingAsync_WithMultiplePassengers_ShouldCalculateTotalCorrectly()
    {
        var flight = MakeFlight("GlobalAir", "US", "US");
        _flightRepoMock.Setup(r => r.GetByIdAsync(flight.Id)).ReturnsAsync(flight);
        _bookingRepoMock.Setup(r => r.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b) => b);
        _bookingRepoMock.Setup(r => r.GetOccupiedSeatsAsync(flight.Id, It.IsAny<DateOnly>())).ReturnsAsync(new List<string>());

        var request = MakeRequest(flight.Id, "AB123456", passengerCount: 3);
        var service = CreateService();

        var result = await service.CreateBookingAsync(request, Guid.NewGuid());

        // GlobalAir: 150 * 1.15 * 3 = 517.50
        result.Pricing.TotalPrice.Should().Be(517.50m);
        result.Pricing.PricePerPassenger.Should().Be(172.50m);
        result.Pricing.NumberOfPassengers.Should().Be(3);
    }

    [Fact]
    public async Task GetBookingByReferenceAsync_WhenOwnerRequests_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        var flight = MakeFlight("GlobalAir", "US", "US");
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingReferenceCode = "SK20260616ABC123",
            UserId = userId,
            FlightId = flight.Id,
            Flight = flight,
            DepartureDate = DateTime.UtcNow.AddDays(1),
            NumberOfPassengers = 1,
            TotalPrice = 172.50m,
            PricePerPassenger = 172.50m,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.UtcNow,
            PassengerDetails =
            [
                new PassengerDetail { FullName = "John Doe", Email = "john@test.com", DocumentType = DocumentType.NationalId, DocumentNumber = "AB123456", PassengerIndex = 1, CreatedAt = DateTime.UtcNow }
            ]
        };
        _bookingRepoMock.Setup(r => r.GetByReferenceCodeAsync("SK20260616ABC123")).ReturnsAsync(booking);

        var result = await CreateService().GetBookingByReferenceAsync("SK20260616ABC123", userId);

        result.BookingReferenceCode.Should().Be("SK20260616ABC123");
    }

    [Fact]
    public async Task GetBookingByReferenceAsync_WhenDifferentUser_ShouldThrow403()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var flight = MakeFlight("GlobalAir", "US", "US");
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingReferenceCode = "SK20260616ABC123",
            UserId = ownerId,
            Flight = flight,
            DepartureDate = DateTime.UtcNow,
            PassengerDetails = []
        };
        _bookingRepoMock.Setup(r => r.GetByReferenceCodeAsync(It.IsAny<string>())).ReturnsAsync(booking);

        var ex = await CreateService().Invoking(s => s.GetBookingByReferenceAsync("SK20260616ABC123", otherUserId))
            .Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(403);
    }
}
