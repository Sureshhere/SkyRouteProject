using System.ComponentModel.DataAnnotations;
using SkyRoute.Domain.Enums;

namespace SkyRoute.Application.DTOs.Flight;

public class FlightSearchRequestDto
{
    [Required]
    [StringLength(10)]
    public string OriginAirportCode { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string DestinationAirportCode { get; set; } = string.Empty;

    [Required]
    public DateOnly DepartureDate { get; set; }

    [Range(1, 9)]
    public int NumberOfPassengers { get; set; } = 1;

    [Required]
    public CabinClass CabinClass { get; set; }
}
