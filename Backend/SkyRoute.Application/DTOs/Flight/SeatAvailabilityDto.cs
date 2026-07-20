namespace SkyRoute.Application.DTOs.Flight;

public class SeatAvailabilityDto
{
    public Guid FlightId { get; set; }
    public DateOnly DepartureDate { get; set; }
    public string CabinClass { get; set; } = string.Empty;
    public IReadOnlyList<string> AvailableSeats { get; set; } = new List<string>();
}
