using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyRoute.Application.DTOs.Booking;
using SkyRoute.Application.Interfaces;
using System.Security.Claims;

namespace SkyRoute.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IValidator<CreateBookingRequestDto> _bookingValidator;

    public BookingsController(IBookingService bookingService, IValidator<CreateBookingRequestDto> bookingValidator)
    {
        _bookingService = bookingService;
        _bookingValidator = bookingValidator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(BookingConfirmationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequestDto request)
    {
        var validation = await _bookingValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        var userId = GetCurrentUserId();
        var result = await _bookingService.CreateBookingAsync(request, userId);
        return CreatedAtAction(nameof(GetByReference), new { referenceCode = result.BookingReferenceCode }, result);
    }

    [HttpGet("{referenceCode}")]
    [ProducesResponseType(typeof(BookingConfirmationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByReference([FromRoute] string referenceCode)
    {
        var userId = GetCurrentUserId();
        var result = await _bookingService.GetBookingByReferenceAsync(referenceCode, userId);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user identity.");

        return userId;
    }
}
