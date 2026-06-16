using SkyRoute.Domain.Models;

namespace SkyRoute.Application.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByReferenceCodeAsync(string referenceCode);
    Task<Booking?> GetByIdAsync(Guid id);
    Task<Booking> AddAsync(Booking booking);
    Task SaveChangesAsync();
}
