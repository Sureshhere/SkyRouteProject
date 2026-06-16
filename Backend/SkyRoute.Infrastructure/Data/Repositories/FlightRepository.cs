using Microsoft.EntityFrameworkCore;
using SkyRoute.Application.Interfaces;
using SkyRoute.Domain.Enums;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Data;

namespace SkyRoute.Infrastructure.Data.Repositories;

public class FlightRepository : IFlightRepository
{
    private readonly SkyRouteDbContext _context;

    public FlightRepository(SkyRouteDbContext context) => _context = context;

    public async Task<IEnumerable<Flight>> GetByRouteAndCabinAsync(Guid originId, Guid destinationId, CabinClass cabinClass) =>
        await _context.Flights
            .Include(f => f.Airline)
            .Include(f => f.OriginAirport)
            .Include(f => f.DestinationAirport)
            .Where(f => f.OriginAirportId == originId
                     && f.DestinationAirportId == destinationId
                     && f.CabinClass == cabinClass
                     && f.IsActive)
            .OrderBy(f => f.DepartureTime)
            .ToListAsync();

    public async Task<Flight?> GetByIdAsync(Guid id) =>
        await _context.Flights
            .Include(f => f.Airline)
            .Include(f => f.OriginAirport)
            .Include(f => f.DestinationAirport)
            .FirstOrDefaultAsync(f => f.Id == id && f.IsActive);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
