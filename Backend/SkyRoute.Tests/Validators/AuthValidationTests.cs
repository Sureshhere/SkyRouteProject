using FluentAssertions;
using FluentValidation;
using SkyRoute.Application.DTOs.Auth;
using SkyRoute.Application.Validators;

namespace SkyRoute.Tests.Validators;

/// <summary>
/// Unit tests for authentication validators covering register and login validation rules.
/// Tests verify email format validation, password requirements, and edge cases.
/// These tests serve as regression tests to ensure validation rules don't break during
/// authentication feature implementation.
/// </summary>
public class AuthValidationTests
{
    private readonly IValidator<RegisterRequestDto> _registerValidator = new RegisterRequestValidator();

    // ── Email Validation Tests (Register) ─────────────────────────────────────

    /// <summary>
    /// Verify that valid email is accepted.
    /// </summary>
    [Fact]
    public async Task RegisterValidator_WithValidEmail_ShouldPass()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "user@example.com",
            Password = "ValidPassword123",
            FullName = "Test User"
        };

        // Act
        var result = await _registerValidator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.PropertyName == "Email");
    }

    /// <summary>
    /// Verify that email is required (not empty/null).
    /// </summary>
    [Fact]
    public async Task RegisterValidator_WithEmptyEmail_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "",
            Password = "ValidPassword123",
            FullName = "Test User"
        };

        // Act
        var result = await _registerValidator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    /// <summary>
    /// Verify that email must be in valid format.
    /// </summary>
    [Theory]
    [InlineData("invalidemail")]
    [InlineData("user@")]
    [InlineData("@example.com")]
    public async Task RegisterValidator_WithInvalidEmailFormat_ShouldFail(string email)
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = email,
            Password = "ValidPassword123",
            FullName = "Test User"
        };

        // Act
        var result = await _registerValidator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse("Email '{email}' should be invalid", email);
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    /// <summary>
    /// Verify that email within max length constraint is valid.
    /// </summary>
    [Fact]
    public async Task RegisterValidator_WithEmailAtMaxLength_ShouldPass()
    {
        // Arrange
        var longLocalPart = new string('a', 64); // Local part within limits
        var email = $"{longLocalPart}@example.com"; // Standard email
        // Most validators allow up to 256 chars for email
        
        if (email.Length <= 256)
        {
            var request = new RegisterRequestDto
            {
                Email = email,
                Password = "ValidPassword123",
                FullName = "Test User"
            };

            // Act
            var result = await _registerValidator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }

    /// <summary>
    /// Verify that email exceeding max length is rejected.
    /// </summary>
    [Fact]
    public async Task RegisterValidator_WithEmailExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var veryLongEmail = new string('a', 200) + "@" + new string('b', 100) + ".com";
        var request = new RegisterRequestDto
        {
            Email = veryLongEmail,
            Password = "ValidPassword123",
            FullName = "Test User"
        };

        // Act
        var result = await _registerValidator.ValidateAsync(request);

        // Assert
        // Should fail due to max length (typically 256 for email)
        if (veryLongEmail.Length > 256)
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }
    }

    // ── Password Validation Tests (Register) ────────────────────────────────────

    /// <summary>
    /// Verify that password is required (not empty).
    /// </summary>
    [Fact]
    public async Task RegisterValidator_WithEmptyPassword_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "user@example.com",
            Password = "",
            FullName = "Test User"
        };

        // Act
        var result = await _registerValidator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    /// <summary>
    /// Verify that password must meet minimum length (typically 8 characters).
    /// </summary>
    [Fact]
    public async Task RegisterValidator_WithPasswordBelowMinLength_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "user@example.com",
            Password = "Pass123",  // 7 characters - below typical 8 char minimum
            FullName = "Test User"
        };

        // Act
        var result = await _registerValidator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    /// <summary>
    /// Verify that password at minimum length is accepted.
    /// </summary>
    [Fact]
    public async Task RegisterValidator_WithPasswordAtMinLength_ShouldPass()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "user@example.com",
            Password = "Pass1234",  // 8 characters
            FullName = "Test User"
        };

        // Act
        var result = await _registerValidator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue("Password of 8 characters should meet minimum length");
    }

    /// <summary>
    /// Verify that password exceeding max length is rejected.
    /// </summary>
    [Fact]
    public async Task RegisterValidator_WithPasswordExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "user@example.com",
            Password = new string('a', 101),  // 101 characters - exceeds typical 100 char max
            FullName = "Test User"
        };

        // Act
        var result = await _registerValidator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    /// <summary>
    /// Verify that password with various character types is accepted.
    /// All passwords must have: uppercase, lowercase, digit, 8+ chars.
    /// </summary>
    [Theory]
    [InlineData("Pass@word123")]
    [InlineData("ValidPassword1")]
    [InlineData("AnotherValid9Password")]
    [InlineData("Special!Pass2")]
    [InlineData("MixedCase123ABC")]
    public async Task RegisterValidator_WithVariousPasswordFormats_ShouldAccept(string password)
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "user@example.com",
            Password = password,
            FullName = "Test User"
        };

        // Act
        var result = await _registerValidator.ValidateAsync(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "Password");
    }

    // ── Full Name Validation Tests (Register) ───────────────────────────────────

    /// <summary>
    /// Verify that full name is required (not empty).
    /// </summary>
    [Fact]
    public async Task RegisterValidator_WithEmptyFullName_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "user@example.com",
            Password = "ValidPassword123",
            FullName = ""
        };

        // Act
        var result = await _registerValidator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    /// <summary>
    /// Verify that full name accepts various character formats.
    /// </summary>
    [Theory]
    [InlineData("John Doe")]
    [InlineData("José García")]
    [InlineData("Maria da Silva")]
    [InlineData("Jean-Pierre Dupont")]
    [InlineData("O'Brien")]
    public async Task RegisterValidator_WithVariousFullNameFormats_ShouldPass(string fullName)
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "user@example.com",
            Password = "ValidPassword123",
            FullName = fullName
        };

        // Act
        var result = await _registerValidator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue("Full name '{fullName}' should be valid", fullName);
    }

    /// <summary>
    /// Verify that full name exceeding max length is rejected.
    /// </summary>
    [Fact]
    public async Task RegisterValidator_WithFullNameExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "user@example.com",
            Password = "ValidPassword123",
            FullName = new string('a', 201)  // Exceeds 200 char max
        };

        // Act
        var result = await _registerValidator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    // ── Multiple Field Validation Tests ─────────────────────────────────────────

    /// <summary>
    /// Verify that all fields are validated in a single validation call.
    /// </summary>
    [Fact]
    public async Task RegisterValidator_WithMultipleErrors_ShouldReportAll()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "invalidemail",
            Password = "short",
            FullName = ""
        };

        // Act
        var result = await _registerValidator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2, "Should report multiple validation failures");
    }

    /// <summary>
    /// Verify that request with all valid fields passes validation.
    /// </summary>
    [Fact]
    public async Task RegisterValidator_WithAllValidFields_ShouldPass()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "valid@example.com",
            Password = "ValidPassword123",
            FullName = "Valid User Name"
        };

        // Act
        var result = await _registerValidator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // ── Regression Tests ────────────────────────────────────────────────────────

    /// <summary>
    /// Regression test: Verify that data annotations still validate email format.
    /// This ensures the [EmailAddress] attribute is respected.
    /// </summary>
    [Fact]
    public async Task RegisterValidator_EmailAddressAttributeStillEnforced()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "not-an-email",
            Password = "ValidPassword123",
            FullName = "User"
        };

        // Act
        var result = await _registerValidator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse("Email must pass [EmailAddress] validation");
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    /// <summary>
    /// Regression test: Verify that password min length from [StringLength] is enforced.
    /// </summary>
    [Fact]
    public async Task RegisterValidator_PasswordMinLengthStillEnforced()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "user@example.com",
            Password = "short",
            FullName = "User"
        };

        // Act
        var result = await _registerValidator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse("Password must meet [StringLength(100, MinimumLength = 8)]");
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    /// <summary>
    /// Regression test: Verify that full name max length is enforced.
    /// </summary>
    [Fact]
    public async Task RegisterValidator_FullNameMaxLengthStillEnforced()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "user@example.com",
            Password = "ValidPassword123",
            FullName = new string('x', 201)
        };

        // Act
        var result = await _registerValidator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse("Full name must meet [StringLength(200)]");
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }
}
