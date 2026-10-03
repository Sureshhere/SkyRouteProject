using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyRoute.Application.DTOs.Flight;
using SkyRoute.Application.Interfaces;

namespace SkyRoute.Api.Controllers;

[ApiController]
[Route("api/flights")]
public class FlightsController : ControllerBase
{
    private readonly IFlightSearchService _flightSearchService;
    private readonly IValidator<FlightSearchRequestDto> _searchValidator;
    private readonly ISeatService _seatService;

    public FlightsController(
        IFlightSearchService flightSearchService,
        IValidator<FlightSearchRequestDto> searchValidator,
        ISeatService seatService)
    {
        _flightSearchService = flightSearchService;
        _searchValidator = searchValidator;
        _seatService = seatService;
    }

    [HttpPost("search")]
    [ProducesResponseType(typeof(FlightSearchResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search([FromBody] FlightSearchRequestDto request)
    {
        var validation = await _searchValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        var result = await _flightSearchService.SearchFlightsAsync(request);
        return Ok(result);
    }

    [HttpGet("{flightId:guid}/seats")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSeats([FromRoute] Guid flightId, [FromQuery] DateOnly departureDate)
    {
        if (departureDate == default)
            return BadRequest(new { error = "Departure date is required." });
        if (departureDate < DateOnly.FromDateTime(DateTime.UtcNow.Date))
            return BadRequest(new { error = "Departure date cannot be in the past." });
        var result = await _seatService.GetAvailableSeatsAsync(flightId, departureDate);
        return Ok(result);
    }
}
