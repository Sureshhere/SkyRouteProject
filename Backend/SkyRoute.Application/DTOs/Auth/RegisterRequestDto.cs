using System.ComponentModel.DataAnnotations;

namespace SkyRoute.Application.DTOs.Auth;

public class RegisterRequestDto
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;
}
