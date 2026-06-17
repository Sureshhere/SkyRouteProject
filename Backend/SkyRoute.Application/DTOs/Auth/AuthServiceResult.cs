namespace SkyRoute.Application.DTOs.Auth;

public class AuthServiceResult
{
    public AuthResponseDto Response { get; init; } = null!;
    public string Token { get; init; } = string.Empty;
}
