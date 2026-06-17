namespace SkyRoute.Application.DTOs.Auth;

public class AuthResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int ExpiresIn { get; set; } = 3600;
}
