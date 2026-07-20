using SkyRoute.Application.Common;
using SkyRoute.Application.DTOs.Booking;
using SkyRoute.Application.Interfaces;
using SkyRoute.Application.Utilities;
using SkyRoute.Application.Validators;
using SkyRoute.Domain.Enums;
using SkyRoute.Domain.Models;

namespace SkyRoute.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IFlightRepository _flightRepository;
    private readonly IEnumerable<IFlightPricingStrategy> _pricingStrategies;

    public BookingService(
        IBookingRepository bookingRepository,
        IFlightRepository flightRepository,
        IEnumerable<IFlightPricingStrategy> pricingStrategies)
    {
        _bookingRepository = bookingRepository;
        _flightRepository = flightRepository;
        _pricingStrategies = pricingStrategies;
    }

    public async Task<BookingConfirmationDto> CreateBookingAsync(CreateBookingRequestDto request, Guid userId)
    {
        var flight = await _flightRepository.GetByIdAsync(request.FlightId)
            ?? throw new AppException("Flight not found.", 404);

        if (request.Passengers.Count is < 1 or > 9)
            throw new AppException("Number of passengers must be between 1 and 9.");

        // Determine document type based on domestic vs international route
        var isDomestic = DocumentNumberValidator.IsDomesticRoute(
            flight.OriginAirport.CountryCode,
            flight.DestinationAirport.CountryCode);

        var documentType = isDomestic ? DocumentType.NationalId : DocumentType.PassportNumber;

        // Validate each passenger's document number
        foreach (var (passenger, index) in request.Passengers.Select((p, i) => (p, i + 1)))
        {
            var isValid = isDomestic
                ? DocumentNumberValidator.IsValidNationalId(passenger.DocumentNumber)
                : DocumentNumberValidator.IsValidPassport(passenger.DocumentNumber);

            if (!isValid)
            {
                var format = isDomestic ? "National ID (8–12 alphanumeric characters)" : "Passport Number (6–9 alphanumeric characters)";
                throw new AppException($"Passenger {index}: Invalid document number. Expected format: {format}.");
            }
        }

        // Layer A — validate each seat is in the cabin's valid seat list
        var validSeats = SeatConfiguration.GetSeatsForCabin(flight.CabinClass.ToString());
        foreach (var p in request.Passengers)
        {
            var normalised = p.SeatNumber.Trim().ToUpperInvariant();
            if (!validSeats.Contains(normalised))
                throw new AppException($"Passenger seat '{p.SeatNumber}' is not valid for cabin class '{flight.CabinClass}'.", 400);
        }

        // Layer B — validate no duplicate seats within the booking
        var seats = request.Passengers.Select(p => p.SeatNumber.Trim().ToUpperInvariant()).ToList();
        if (seats.Distinct().Count() != seats.Count)
            throw new AppException("Two or more passengers cannot be assigned the same seat.", 400);

        // Layer C — validate no seat is already Confirmed for this flight/date
        var occupied = await _bookingRepository.GetOccupiedSeatsAsync(request.FlightId, request.DepartureDate);
        foreach (var p in request.Passengers)
        {
            var normalised = p.SeatNumber.Trim().ToUpperInvariant();
            if (occupied.Contains(normalised, StringComparer.OrdinalIgnoreCase))
                throw new AppException($"Seat '{p.SeatNumber}' is already taken.", 409);
        }

        // Calculate pricing using the registered strategy for this airline
        var strategy = _pricingStrategies.FirstOrDefault(s => s.ProviderName == flight.Airline.Name)
            ?? throw new AppException($"Pricing strategy not found for airline '{flight.Airline.Name}'.", 500);

        var pricing = strategy.CalculatePrice(flight.BaseFare, request.Passengers.Count);

        var departureDateTime = request.DepartureDate.ToDateTime(TimeOnly.FromTimeSpan(flight.DepartureTime));
        var arrivalDateTime = departureDateTime.AddMinutes(flight.DurationMinutes);
        var referenceCode = GenerateReferenceCode();

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingReferenceCode = referenceCode,
            UserId = userId,
            FlightId = flight.Id,
            DepartureDate = departureDateTime,
            NumberOfPassengers = request.Passengers.Count,
            TotalPrice = pricing.TotalPrice,
            PricePerPassenger = pricing.PricePerPassenger,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PassengerDetails = request.Passengers.Select((p, i) => new PassengerDetail
            {
                Id = Guid.NewGuid(),
                FullName = p.FullName.Trim(),
                Email = p.Email.ToLowerInvariant(),
                DocumentType = documentType,
                DocumentNumber = p.DocumentNumber.ToUpperInvariant(),
                SeatNumber = p.SeatNumber.Trim().ToUpperInvariant(),
                PassengerIndex = i + 1,
                CreatedAt = DateTime.UtcNow
            }).ToList()
        };

        await _bookingRepository.AddAsync(booking);
        await _bookingRepository.SaveChangesAsync();

        return MapToConfirmationDto(booking, flight, pricing.TotalPrice, pricing.PricePerPassenger, departureDateTime, arrivalDateTime);
    }

    public async Task<BookingConfirmationDto> GetBookingByReferenceAsync(string referenceCode, Guid userId)
    {
        var booking = await _bookingRepository.GetByReferenceCodeAsync(referenceCode)
            ?? throw new AppException("Booking not found.", 404);

        if (booking.UserId != userId)
            throw new AppException("You are not authorized to view this booking.", 403);

        var departureDateTime = booking.DepartureDate;
        var arrivalDateTime = departureDateTime.AddMinutes(booking.Flight.DurationMinutes);

        return MapToConfirmationDto(booking, booking.Flight, booking.TotalPrice, booking.PricePerPassenger, departureDateTime, arrivalDateTime);
    }

    private static BookingConfirmationDto MapToConfirmationDto(
        Booking booking, Flight flight,
        decimal totalPrice, decimal pricePerPassenger,
        DateTime departureDateTime, DateTime arrivalDateTime)
    {
        return new BookingConfirmationDto
        {
            BookingId = booking.Id,
            BookingReferenceCode = booking.BookingReferenceCode,
            FlightDetails = new BookingFlightSummaryDto
            {
                AirlineName = flight.Airline.Name,
                FlightNumber = flight.FlightNumber,
                Origin = flight.OriginAirport.Code,
                Destination = flight.DestinationAirport.Code,
                DepartureTime = departureDateTime,
                ArrivalTime = arrivalDateTime,
                CabinClass = flight.CabinClass.ToString()
            },
            Pricing = new BookingPricingDto
            {
                TotalPrice = totalPrice,
                PricePerPassenger = pricePerPassenger,
                NumberOfPassengers = booking.PassengerDetails.Count
            },
            Passengers = booking.PassengerDetails.OrderBy(p => p.PassengerIndex).Select(p => new PassengerSummaryDto
            {
                FullName = p.FullName,
                Email = p.Email,
                DocumentType = p.DocumentType.ToString(),
                SeatNumber = p.SeatNumber
            }),
            BookingStatus = booking.Status.ToString(),
            CreatedAt = booking.CreatedAt
        };
    }

    private static string GenerateReferenceCode()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"SK{datePart}{randomPart}";
    }
}
