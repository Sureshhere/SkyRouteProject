using SkyRoute.Application.DTOs.Airport;

namespace SkyRoute.Application.Interfaces;

public interface IAirportService
{
    Task<IEnumerable<AirportDto>> GetAllAirportsAsync();
    Task<AirportDto?> GetAirportByCodeAsync(string code);
}
