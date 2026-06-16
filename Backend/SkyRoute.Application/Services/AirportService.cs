using SkyRoute.Application.DTOs.Airport;
using SkyRoute.Application.Interfaces;

namespace SkyRoute.Application.Services;

public class AirportService : IAirportService
{
    private readonly IAirportRepository _airportRepository;

    public AirportService(IAirportRepository airportRepository)
    {
        _airportRepository = airportRepository;
    }

    public async Task<IEnumerable<AirportDto>> GetAllAirportsAsync()
    {
        var airports = await _airportRepository.GetAllAsync();
        return airports.Select(a => new AirportDto
        {
            Id = a.Id,
            Code = a.Code,
            Name = a.Name,
            City = a.City,
            Country = a.Country,
            CountryCode = a.CountryCode
        });
    }

    public async Task<AirportDto?> GetAirportByCodeAsync(string code)
    {
        var airport = await _airportRepository.GetByCodeAsync(code.ToUpperInvariant());
        if (airport == null) return null;

        return new AirportDto
        {
            Id = airport.Id,
            Code = airport.Code,
            Name = airport.Name,
            City = airport.City,
            Country = airport.Country,
            CountryCode = airport.CountryCode
        };
    }
}
