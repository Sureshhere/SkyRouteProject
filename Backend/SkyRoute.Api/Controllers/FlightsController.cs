using FluentValidation;
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

    public FlightsController(IFlightSearchService flightSearchService, IValidator<FlightSearchRequestDto> searchValidator)
    {
        _flightSearchService = flightSearchService;
        _searchValidator = searchValidator;
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
}
