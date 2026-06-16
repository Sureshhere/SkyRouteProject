using SkyRoute.Application.Common;
using SkyRoute.Application.DTOs.Auth;
using SkyRoute.Application.Interfaces;
using SkyRoute.Domain.Models;

namespace SkyRoute.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenProvider _jwtTokenProvider;

    public AuthService(IUserRepository userRepository, IJwtTokenProvider jwtTokenProvider)
    {
        _userRepository = userRepository;
        _jwtTokenProvider = jwtTokenProvider;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new AppException("Email is already registered.", 409);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
            FullName = request.FullName.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var token = _jwtTokenProvider.GenerateToken(user);

        return new AuthResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Token = token,
            ExpiresIn = 3600
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.ToLowerInvariant());
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new AppException("Invalid email or password.", 401);

        if (!user.IsActive)
            throw new AppException("Account is disabled. Please contact support.", 403);

        var token = _jwtTokenProvider.GenerateToken(user);

        return new AuthResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Token = token,
            ExpiresIn = 3600
        };
    }
}
