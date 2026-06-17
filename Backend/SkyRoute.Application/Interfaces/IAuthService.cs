using SkyRoute.Application.DTOs.Auth;

namespace SkyRoute.Application.Interfaces;

public interface IAuthService
{
    Task<AuthServiceResult> RegisterAsync(RegisterRequestDto request);
    Task<AuthServiceResult> LoginAsync(LoginRequestDto request);
}
