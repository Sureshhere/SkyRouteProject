using Microsoft.EntityFrameworkCore;
using SkyRoute.Application.Interfaces;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Data;

namespace SkyRoute.Infrastructure.Data.Repositories;

public class AirportRepository : IAirportRepository
{
    private readonly SkyRouteDbContext _context;

    public AirportRepository(SkyRouteDbContext context) => _context = context;

    public async Task<IEnumerable<Airport>> GetAllAsync() =>
        await _context.Airports.Where(a => a.IsActive).OrderBy(a => a.Country).ThenBy(a => a.City).ToListAsync();

    public async Task<Airport?> GetByCodeAsync(string code) =>
        await _context.Airports.FirstOrDefaultAsync(a => a.Code == code && a.IsActive);

    public async Task<Airport?> GetByIdAsync(Guid id) =>
        await _context.Airports.FirstOrDefaultAsync(a => a.Id == id && a.IsActive);
}
