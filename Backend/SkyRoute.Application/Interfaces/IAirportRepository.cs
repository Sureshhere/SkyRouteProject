using SkyRoute.Domain.Models;

namespace SkyRoute.Application.Interfaces;

public interface IAirportRepository
{
    Task<IEnumerable<Airport>> GetAllAsync();
    Task<Airport?> GetByCodeAsync(string code);
    Task<Airport?> GetByIdAsync(Guid id);
}
