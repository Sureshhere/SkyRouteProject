using FluentAssertions;
using Moq;
using SkyRoute.Application.Common;
using SkyRoute.Application.DTOs.Auth;
using SkyRoute.Application.Interfaces;
using SkyRoute.Application.Services;
using SkyRoute.Domain.Models;

namespace SkyRoute.Tests.Services;

/// <summary>
/// Unit tests for AuthService covering login, register, and token generation scenarios.
/// Tests verify business logic, error handling, password hashing, and token inclusion.
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IJwtTokenProvider> _jwtProviderMock = new();

    private AuthService CreateService() =>
        new(_userRepoMock.Object, _jwtProviderMock.Object);

    private static User MakeUser(string email = "user@example.com", bool isActive = true, string passwordHash = "") =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            FullName = "Test User",
            PasswordHash = string.IsNullOrEmpty(passwordHash) ? BCrypt.Net.BCrypt.HashPassword("ValidPass123", workFactor: 12) : passwordHash,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };

    // ── Register Tests ──────────────────────────────────────────────────────────
    
    /// <summary>
    /// Verify successful user registration creates a user with proper fields,
    /// generates a token, and returns AuthResponseDto.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "newuser@example.com",
            Password = "ValidPassword123",
            FullName = "New User"
        };
        
        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email.ToLowerInvariant()))
            .ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        _jwtProviderMock.Setup(p => p.GenerateToken(It.IsAny<User>()))
            .Returns("fake-jwt-token");

        var service = CreateService();

        // Act
        var result = await service.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(request.Email.ToLowerInvariant());
        result.FullName.Should().Be(request.FullName.Trim());
        result.Id.Should().NotBeEmpty();
        result.Token.Should().Be("fake-jwt-token");
        result.ExpiresIn.Should().Be(3600);
        
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _userRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        _jwtProviderMock.Verify(p => p.GenerateToken(It.IsAny<User>()), Times.Once);
    }

    /// <summary>
    /// Verify that registering with duplicate email throws 409 Conflict exception.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ShouldThrow409()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "existing@example.com",
            Password = "ValidPassword123",
            FullName = "User"
        };
        
        var existingUser = MakeUser(request.Email);
        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email.ToLowerInvariant()))
            .ReturnsAsync(existingUser);

        var service = CreateService();

        // Act & Assert
        await service.Invoking(s => s.RegisterAsync(request))
            .Should().ThrowAsync<AppException>()
            .WithMessage("*already registered*")
            .Where(e => e.StatusCode == 409);
    }

    /// <summary>
    /// Verify that registered user has password properly hashed (not stored in plain text).
    /// </summary>
    [Fact]
    public async Task RegisterAsync_ShouldHashPasswordWithBCrypt()
    {
        // Arrange
        var password = "TestPassword123!";
        var request = new RegisterRequestDto
        {
            Email = "user@example.com",
            Password = password,
            FullName = "User"
        };

        User capturedUser = null!;
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync((User u) => u);
        _jwtProviderMock.Setup(p => p.GenerateToken(It.IsAny<User>()))
            .Returns("token");

        var service = CreateService();

        // Act
        await service.RegisterAsync(request);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser.PasswordHash.Should().NotBe(password); // Not plain text
        capturedUser.PasswordHash.Should().Match("$2*$12$*"); // BCrypt format with workFactor 12 (matches $2a$ or $2b$)
        BCrypt.Net.BCrypt.Verify(password, capturedUser.PasswordHash).Should().BeTrue();
    }

    /// <summary>
    /// Verify that email is normalized to lowercase during registration.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_ShouldNormalizeEmailToLowercase()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "User@EXAMPLE.com",
            Password = "ValidPassword123",
            FullName = "User"
        };

        User capturedUser = null!;
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync((User u) => u);
        _jwtProviderMock.Setup(p => p.GenerateToken(It.IsAny<User>()))
            .Returns("token");

        var service = CreateService();

        // Act
        var result = await service.RegisterAsync(request);

        // Assert
        result.Email.Should().Be("user@example.com");
        capturedUser.Email.Should().Be("user@example.com");
    }

    /// <summary>
    /// Verify that full name is trimmed during registration.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_ShouldTrimFullName()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "user@example.com",
            Password = "ValidPassword123",
            FullName = "  User With Spaces  "
        };

        User capturedUser = null!;
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync((User u) => u);
        _jwtProviderMock.Setup(p => p.GenerateToken(It.IsAny<User>()))
            .Returns("token");

        var service = CreateService();

        // Act
        var result = await service.RegisterAsync(request);

        // Assert
        result.FullName.Should().Be("User With Spaces");
        capturedUser.FullName.Should().Be("User With Spaces");
    }

    // ── Login Tests ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Verify successful login with valid credentials returns user info and token.
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        var password = "ValidPassword123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        var user = MakeUser(passwordHash: passwordHash);
        
        var request = new LoginRequestDto
        {
            Email = user.Email,
            Password = password
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email.ToLowerInvariant()))
            .ReturnsAsync(user);
        _jwtProviderMock.Setup(p => p.GenerateToken(user))
            .Returns("fake-jwt-token");

        var service = CreateService();

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.FullName.Should().Be(user.FullName);
        result.Token.Should().Be("fake-jwt-token");
        result.ExpiresIn.Should().Be(3600);
    }

    /// <summary>
    /// Verify login with non-existent email throws 401 Unauthorized.
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithNonexistentEmail_ShouldThrow401()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "nonexistent@example.com",
            Password = "SomePassword123"
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email.ToLowerInvariant()))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        // Act & Assert
        await service.Invoking(s => s.LoginAsync(request))
            .Should().ThrowAsync<AppException>()
            .WithMessage("*Invalid email or password*")
            .Where(e => e.StatusCode == 401);
    }

    /// <summary>
    /// Verify login with invalid password throws 401 Unauthorized.
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrow401()
    {
        // Arrange
        var user = MakeUser();
        var request = new LoginRequestDto
        {
            Email = user.Email,
            Password = "WrongPassword123"
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email.ToLowerInvariant()))
            .ReturnsAsync(user);

        var service = CreateService();

        // Act & Assert
        await service.Invoking(s => s.LoginAsync(request))
            .Should().ThrowAsync<AppException>()
            .WithMessage("*Invalid email or password*")
            .Where(e => e.StatusCode == 401);
    }

    /// <summary>
    /// Verify login with inactive user throws 403 Forbidden.
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldThrow403()
    {
        // Arrange
        var password = "ValidPassword123";
        var user = MakeUser(isActive: false, passwordHash: BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12));
        
        var request = new LoginRequestDto
        {
            Email = user.Email,
            Password = password
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email.ToLowerInvariant()))
            .ReturnsAsync(user);

        var service = CreateService();

        // Act & Assert
        await service.Invoking(s => s.LoginAsync(request))
            .Should().ThrowAsync<AppException>()
            .WithMessage("*disabled*")
            .Where(e => e.StatusCode == 403);
    }

    /// <summary>
    /// Verify login normalizes email to lowercase before lookup.
    /// </summary>
    [Fact]
    public async Task LoginAsync_ShouldNormalizeEmailToLowercase()
    {
        // Arrange
        var password = "ValidPassword123";
        var user = MakeUser("user@example.com", passwordHash: BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12));
        
        var request = new LoginRequestDto
        {
            Email = "USER@EXAMPLE.COM",
            Password = password
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("user@example.com"))
            .ReturnsAsync(user);
        _jwtProviderMock.Setup(p => p.GenerateToken(user))
            .Returns("token");

        var service = CreateService();

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        _userRepoMock.Verify(r => r.GetByEmailAsync("user@example.com"), Times.Once);
    }

    // ── Response DTO Tests ──────────────────────────────────────────────────────

    /// <summary>
    /// Verify that AuthResponseDto contains token (for integration purposes).
    /// Token is returned by service but should be handled securely at controller level.
    /// </summary>
    [Fact]
    public async Task LoginAsync_ResponseIncludesToken()
    {
        // Arrange
        var password = "ValidPassword123";
        var user = MakeUser(passwordHash: BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12));
        var testToken = "test-jwt-token-12345";
        
        var request = new LoginRequestDto
        {
            Email = user.Email,
            Password = password
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email.ToLowerInvariant()))
            .ReturnsAsync(user);
        _jwtProviderMock.Setup(p => p.GenerateToken(user))
            .Returns(testToken);

        var service = CreateService();

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        result.Token.Should().Be(testToken);
    }

    /// <summary>
    /// Verify RegisterAsync response includes token (for integration purposes).
    /// </summary>
    [Fact]
    public async Task RegisterAsync_ResponseIncludesToken()
    {
        // Arrange
        var testToken = "test-jwt-token-67890";
        var request = new RegisterRequestDto
        {
            Email = "new@example.com",
            Password = "ValidPassword123",
            FullName = "New User"
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        _jwtProviderMock.Setup(p => p.GenerateToken(It.IsAny<User>()))
            .Returns(testToken);

        var service = CreateService();

        // Act
        var result = await service.RegisterAsync(request);

        // Assert
        result.Token.Should().Be(testToken);
    }
}
