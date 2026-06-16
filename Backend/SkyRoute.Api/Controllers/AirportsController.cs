using Microsoft.AspNetCore.Mvc;
using SkyRoute.Application.DTOs.Airport;
using SkyRoute.Application.Interfaces;

namespace SkyRoute.Api.Controllers;

[ApiController]
[Route("api/airports")]
public class AirportsController : ControllerBase
{
    private readonly IAirportService _airportService;

    public AirportsController(IAirportService airportService) => _airportService = airportService;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AirportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var airports = await _airportService.GetAllAirportsAsync();
        return Ok(airports);
    }
}
