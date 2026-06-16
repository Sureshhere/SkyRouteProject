using Microsoft.EntityFrameworkCore;
using SkyRoute.Application.Interfaces;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Data;

namespace SkyRoute.Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SkyRouteDbContext _context;

    public UserRepository(SkyRouteDbContext context) => _context = context;

    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

    public async Task<User?> GetByIdAsync(Guid id) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);

    public async Task<User> AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        return user;
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
