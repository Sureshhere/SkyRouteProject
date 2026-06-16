using SkyRoute.Domain.Models;

namespace SkyRoute.Application.Interfaces;

public interface IJwtTokenProvider
{
    string GenerateToken(User user);
}
