using FluentAssertions;
using SkyRoute.Application.Validators;

namespace SkyRoute.Tests.Validators;

public class DocumentNumberValidatorTests
{
    // ── National ID (domestic) ────────────────────────────────────────────────

    [Theory]
    [InlineData("AB123456")]    // 8 chars — min valid
    [InlineData("ABC1234567")]  // 10 chars
    [InlineData("ABCD12345678")] // 12 chars — max valid
    [InlineData("12345678")]    // 8 digits only
    public void IsValidNationalId_WithValidFormats_ShouldReturnTrue(string documentNumber)
    {
        DocumentNumberValidator.IsValidNationalId(documentNumber).Should().BeTrue();
    }

    [Theory]
    [InlineData("AB1234")]       // 6 chars — too short
    [InlineData("ABCD123456789")] // 13 chars — too long
    [InlineData("AB 12345")]    // space — invalid character
    [InlineData("AB-12345")]    // dash — invalid character
    [InlineData("")]             // empty
    [InlineData(null)]           // null
    public void IsValidNationalId_WithInvalidFormats_ShouldReturnFalse(string? documentNumber)
    {
        DocumentNumberValidator.IsValidNationalId(documentNumber!).Should().BeFalse();
    }

    // ── Passport (international) ──────────────────────────────────────────────

    [Theory]
    [InlineData("A12345B")]   // 7 chars
    [InlineData("AB1234")]    // 6 chars — min valid
    [InlineData("A12345678")] // 9 chars — max valid
    [InlineData("123456")]    // 6 digits only
    public void IsValidPassport_WithValidFormats_ShouldReturnTrue(string documentNumber)
    {
        DocumentNumberValidator.IsValidPassport(documentNumber).Should().BeTrue();
    }

    [Theory]
    [InlineData("A1234")]      // 5 chars — too short
    [InlineData("A1234567890")] // 10 chars — too long
    [InlineData("AB 1234")]   // space — invalid
    [InlineData("AB-1234")]   // dash — invalid
    [InlineData("")]            // empty
    [InlineData(null)]          // null
    public void IsValidPassport_WithInvalidFormats_ShouldReturnFalse(string? documentNumber)
    {
        DocumentNumberValidator.IsValidPassport(documentNumber!).Should().BeFalse();
    }

    // ── Domestic Route Detection ──────────────────────────────────────────────

    [Theory]
    [InlineData("US", "US", true)]   // same country — domestic
    [InlineData("IN", "IN", true)]   // same country — domestic
    [InlineData("GB", "GB", true)]   // same country — domestic
    [InlineData("US", "GB", false)]  // different country — international
    [InlineData("US", "IN", false)]  // different country — international
    [InlineData("GB", "IN", false)]  // different country — international
    [InlineData("us", "US", true)]   // case-insensitive
    public void IsDomesticRoute_ShouldDetectCorrectly(string originCode, string destCode, bool expectedResult)
    {
        DocumentNumberValidator.IsDomesticRoute(originCode, destCode).Should().Be(expectedResult);
    }
}
