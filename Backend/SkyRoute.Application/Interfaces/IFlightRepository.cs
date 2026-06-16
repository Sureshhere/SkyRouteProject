using SkyRoute.Domain.Enums;
using SkyRoute.Domain.Models;

namespace SkyRoute.Application.Interfaces;

public interface IFlightRepository
{
    Task<IEnumerable<Flight>> GetByRouteAndCabinAsync(Guid originId, Guid destinationId, CabinClass cabinClass);
    Task<Flight?> GetByIdAsync(Guid id);
    Task SaveChangesAsync();
}
