using SkyRoute.Application.DTOs.Flight;

namespace SkyRoute.Application.Interfaces;

public interface ISeatService
{
    Task<SeatAvailabilityDto> GetAvailableSeatsAsync(Guid flightId, DateOnly departureDate);
}
