using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SkyRoute.Api.Controllers;
using SkyRoute.Application.Common;
using SkyRoute.Application.DTOs.Booking;
using SkyRoute.Application.Interfaces;
using System.Security.Claims;

namespace SkyRoute.Tests.Controllers;

/// <summary>
/// Unit tests for BookingsController authorization and authentication.
/// Tests verify that the [Authorize] attribute protects endpoints and that
/// authenticated requests are properly processed.
/// </summary>
public class BookingControllerAuthTests
{
    private readonly Mock<IBookingService> _bookingServiceMock = new();
    private readonly Mock<IValidator<CreateBookingRequestDto>> _validatorMock = new();

    private BookingsController CreateController(ClaimsPrincipal? user = null)
    {
        var controller = new BookingsController(_bookingServiceMock.Object, _validatorMock.Object);
        
        // Always create HttpContext, either with or without authenticated user
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        if (user != null)
        {
            httpContext.User = user;
        }
        else
        {
            // Create unauthenticated context with empty Claims principal
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        }
        
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    private static CreateBookingRequestDto MakeBookingRequest() =>
        new()
        {
            FlightId = Guid.NewGuid(),
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            Passengers = new List<PassengerInputDto>
            {
                new()
                {
                    FullName = "Test Passenger",
                    Email = "passenger@example.com",
                    DocumentNumber = "AB123456"
                }
            }
        };

    private static BookingConfirmationDto MakeBookingConfirmation() =>
        new()
        {
            BookingId = Guid.NewGuid(),
            BookingReferenceCode = "SK123456",
            CreatedAt = DateTime.UtcNow,
            BookingStatus = "Confirmed",
            FlightDetails = new BookingFlightSummaryDto
            {
                AirlineName = "GlobalAir",
                FlightNumber = "GA123",
                Origin = "JFK",
                Destination = "LHR",
                DepartureTime = DateTime.UtcNow.AddHours(2),
                ArrivalTime = DateTime.UtcNow.AddHours(8),
                CabinClass = "Economy"
            },
            Passengers = new List<PassengerSummaryDto>
            {
                new()
                {
                    FullName = "Test Passenger",
                    Email = "passenger@example.com",
                    DocumentType = "NationalId"
                }
            },
            Pricing = new BookingPricingDto
            {
                TotalPrice = 500.00m,
                PricePerPassenger = 500.00m,
                NumberOfPassengers = 1
            }
        };

    private static ClaimsPrincipal CreateAuthenticatedUser(Guid userId = default)
    {
        var actualUserId = userId == default ? Guid.NewGuid() : userId;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, actualUserId.ToString()),
            new(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, "user@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestScheme");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CreateAuthenticatedUserWithSubClaim(Guid userId = default)
    {
        var actualUserId = userId == default ? Guid.NewGuid() : userId;
        var claims = new List<Claim>
        {
            new("sub", actualUserId.ToString()),
            new(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, "user@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestScheme");
        return new ClaimsPrincipal(identity);
    }

    // ── Authorization Tests ─────────────────────────────────────────────────────

    /// <summary>
    /// Verify that BookingsController has [Authorize] attribute at class level.
    /// This means all endpoints require authentication.
    /// </summary>
    [Fact]
    public void BookingsController_ShouldHaveAuthorizeAttribute()
    {
        // Act & Assert
        var authorizeAttribute = typeof(BookingsController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .FirstOrDefault() as AuthorizeAttribute;

        authorizeAttribute.Should().NotBeNull("BookingsController should require authorization");
    }

    /// <summary>
    /// Verify that CreateBooking endpoint requires authentication.
    /// Without valid JWT in Authorization header or cookie, request should fail.
    /// </summary>
    [Fact]
    public async Task CreateBooking_WithoutAuthentication_ShouldRequireAuth()
    {
        // Arrange
        var request = MakeBookingRequest();
        var controller = CreateController(); // No authenticated user
        
        // Set up validator to return valid result for any input
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateBookingRequestDto>(), default))
            .ReturnsAsync(new ValidationResult());

        // Act & Assert
        // Without authentication, controller should throw UnauthorizedAccessException
        // when trying to get current user ID
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await controller.CreateBooking(request)
        );
        
        Assert.NotNull(exception);
    }

    /// <summary>
    /// Verify that CreateBooking with valid authentication extracts user ID from claims.
    /// </summary>
    [Fact]
    public async Task CreateBooking_WithValidAuthentication_ShouldExtractUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = MakeBookingRequest();
        var confirmation = MakeBookingConfirmation();

        var user = CreateAuthenticatedUser(userId);

        _validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _bookingServiceMock.Setup(s => s.CreateBookingAsync(request, userId))
            .ReturnsAsync(confirmation);

        var controller = CreateController(user);

        // Act
        var result = await controller.CreateBooking(request);

        // Assert
        _bookingServiceMock.Verify(
            s => s.CreateBookingAsync(request, userId),
            Times.Once,
            "Service should be called with extracted user ID");
    }

    /// <summary>
    /// Verify that GetByReference endpoint requires authentication.
    /// </summary>
    [Fact]
    public async Task GetByReference_WithoutAuthentication_ShouldRequireAuth()
    {
        // Arrange
        var controller = CreateController(); // No authenticated user

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await controller.GetByReference("SK123456")
        );
        
        Assert.NotNull(exception);
    }

    /// <summary>
    /// Verify that GetByReference with valid authentication extracts user ID.
    /// </summary>
    [Fact]
    public async Task GetByReference_WithValidAuthentication_ShouldExtractUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var referenceCode = "SK123456";
        var confirmation = MakeBookingConfirmation();

        var user = CreateAuthenticatedUser(userId);

        _bookingServiceMock.Setup(s => s.GetBookingByReferenceAsync(referenceCode, userId))
            .ReturnsAsync(confirmation);

        var controller = CreateController(user);

        // Act
        var result = await controller.GetByReference(referenceCode);

        // Assert
        _bookingServiceMock.Verify(
            s => s.GetBookingByReferenceAsync(referenceCode, userId),
            Times.Once,
            "Service should be called with extracted user ID");
        
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
    }

    // ── User ID Extraction Tests ────────────────────────────────────────────────

    /// <summary>
    /// Verify that user ID is extracted from NameIdentifier claim (standard claim).
    /// </summary>
    [Fact]
    public async Task CreateBooking_ExtractsUserIdFromNameIdentifierClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = MakeBookingRequest();
        var confirmation = MakeBookingConfirmation();

        var user = CreateAuthenticatedUser(userId);

        _validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _bookingServiceMock.Setup(s => s.CreateBookingAsync(request, userId))
            .ReturnsAsync(confirmation);

        var controller = CreateController(user);

        // Act
        var result = await controller.CreateBooking(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        _bookingServiceMock.Verify(
            s => s.CreateBookingAsync(It.IsAny<CreateBookingRequestDto>(), userId),
            Times.Once);
    }

    /// <summary>
    /// Verify that user ID is extracted from "sub" claim as fallback (JWT standard claim).
    /// </summary>
    [Fact]
    public async Task CreateBooking_ExtractsUserIdFromSubClaimAsFallback()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = MakeBookingRequest();
        var confirmation = MakeBookingConfirmation();

        var user = CreateAuthenticatedUserWithSubClaim(userId);

        _validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _bookingServiceMock.Setup(s => s.CreateBookingAsync(request, userId))
            .ReturnsAsync(confirmation);

        var controller = CreateController(user);

        // Act
        var result = await controller.CreateBooking(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        _bookingServiceMock.Verify(
            s => s.CreateBookingAsync(It.IsAny<CreateBookingRequestDto>(), userId),
            Times.Once);
    }

    /// <summary>
    /// Verify that invalid user ID in claims throws UnauthorizedAccessException.
    /// </summary>
    [Fact]
    public async Task CreateBooking_WithInvalidUserIdClaim_ShouldThrowUnauthorizedAccess()
    {
        // Arrange
        var request = MakeBookingRequest();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "not-a-guid")
        };
        var identity = new ClaimsIdentity(claims, "TestScheme");
        var user = new ClaimsPrincipal(identity);

        // Set up validator to return valid result
        _validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());

        var controller = CreateController(user);

        // Act & Assert
        await controller.Invoking(c => c.CreateBooking(request))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    /// <summary>
    /// Verify that missing user ID claims throws UnauthorizedAccessException.
    /// </summary>
    [Fact]
    public async Task CreateBooking_WithoutUserIdClaim_ShouldThrowUnauthorizedAccess()
    {
        // Arrange
        var request = MakeBookingRequest();
        var claims = new List<Claim>
        {
            new(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, "user@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestScheme");
        var user = new ClaimsPrincipal(identity);

        // Set up validator to return valid result
        _validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());

        var controller = CreateController(user);

        // Act & Assert
        await controller.Invoking(c => c.CreateBooking(request))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ── Request Validation Tests ────────────────────────────────────────────────

    /// <summary>
    /// Verify that CreateBooking validates request before processing.
    /// </summary>
    [Fact]
    public async Task CreateBooking_ValidatesRequestBeforeProcessing()
    {
        // Arrange
        var request = MakeBookingRequest();
        var user = CreateAuthenticatedUser();

        var validationErrors = new List<ValidationFailure>
        {
            new("DepartureDate", "Departure date is required")
        };
        var validationResult = new ValidationResult(validationErrors);

        _validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(validationResult);

        var controller = CreateController(user);

        // Act
        var result = await controller.CreateBooking(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _bookingServiceMock.Verify(
            s => s.CreateBookingAsync(It.IsAny<CreateBookingRequestDto>(), It.IsAny<Guid>()),
            Times.Never,
            "Service should not be called if validation fails");
    }

    /// <summary>
    /// Verify that CreateBooking returns 201 Created with valid input and authentication.
    /// </summary>
    [Fact]
    public async Task CreateBooking_WithValidInputAndAuth_ShouldReturn201Created()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = MakeBookingRequest();
        var confirmation = MakeBookingConfirmation();

        var user = CreateAuthenticatedUser(userId);

        _validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _bookingServiceMock.Setup(s => s.CreateBookingAsync(request, userId))
            .ReturnsAsync(confirmation);

        var controller = CreateController(user);

        // Act
        var result = await controller.CreateBooking(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult.StatusCode.Should().Be(201);
    }

    // ── Response Status Codes ───────────────────────────────────────────────────

    /// <summary>
    /// Verify that GetByReference returns 200 OK when booking is found.
    /// </summary>
    [Fact]
    public async Task GetByReference_WithValidReference_ShouldReturn200Ok()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var referenceCode = "SK123456";
        var confirmation = MakeBookingConfirmation();

        var user = CreateAuthenticatedUser(userId);

        _bookingServiceMock.Setup(s => s.GetBookingByReferenceAsync(referenceCode, userId))
            .ReturnsAsync(confirmation);

        var controller = CreateController(user);

        // Act
        var result = await controller.GetByReference(referenceCode);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// Regression test: Verify that public endpoints (if any) are not affected by auth changes.
    /// Note: In the current implementation, only [Authorize] is used for bookings.
    /// </summary>
    [Fact]
    public void BookingControllerAuthRegression_OnlyBookingEndpointsRequireAuth()
    {
        // This is a specification test documenting that all booking endpoints require auth
        var authorizeAttr = typeof(BookingsController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true);

        authorizeAttr.Should().NotBeEmpty("All booking operations should require authentication");
    }
}
