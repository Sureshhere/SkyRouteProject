using SkyRoute.Application.DTOs.Flight;

namespace SkyRoute.Application.Interfaces;

public interface IFlightSearchService
{
    Task<FlightSearchResponseDto> SearchFlightsAsync(FlightSearchRequestDto request);
}
