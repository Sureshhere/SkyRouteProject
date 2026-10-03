using SkyRoute.Application.Common;
using SkyRoute.Application.DTOs.Flight;
using SkyRoute.Application.Interfaces;
using SkyRoute.Application.Utilities;

namespace SkyRoute.Application.Services;

public class SeatService : ISeatService
{
    private readonly IFlightRepository _flightRepository;
    private readonly IBookingRepository _bookingRepository;

    public SeatService(IFlightRepository flightRepository, IBookingRepository bookingRepository)
    {
        _flightRepository = flightRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<SeatAvailabilityDto> GetAvailableSeatsAsync(Guid flightId, DateOnly departureDate)
    {
        var flight = await _flightRepository.GetByIdAsync(flightId)
            ?? throw new AppException("Flight not found.", 404);

        var allSeats = SeatConfiguration.GetSeatsForCabin(flight.CabinClass.ToString());
        var occupiedSeats = await _bookingRepository.GetOccupiedSeatsAsync(flightId, departureDate);

        var availableSeats = allSeats
            .Except(occupiedSeats, StringComparer.OrdinalIgnoreCase)
            .ToList()
            .AsReadOnly();

        return new SeatAvailabilityDto
        {
            FlightId = flightId,
            DepartureDate = departureDate,
            CabinClass = flight.CabinClass.ToString(),
            AvailableSeats = availableSeats
        };
    }
}
