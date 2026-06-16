using SkyRoute.Domain.Models;

namespace SkyRoute.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<User> AddAsync(User user);
    Task SaveChangesAsync();
}
