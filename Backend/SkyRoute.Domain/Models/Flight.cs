using SkyRoute.Domain.Enums;

namespace SkyRoute.Domain.Models;

public class Flight
{
    public Guid Id { get; set; }
    public Guid AirlineId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public Guid OriginAirportId { get; set; }
    public Guid DestinationAirportId { get; set; }
    public TimeSpan DepartureTime { get; set; }
    public TimeSpan ArrivalTime { get; set; }
    public int DurationMinutes { get; set; }
    public CabinClass CabinClass { get; set; }
    public decimal BaseFare { get; set; }
    public bool IsActive { get; set; } = true;

    public Airline Airline { get; set; } = null!;
    public Airport OriginAirport { get; set; } = null!;
    public Airport DestinationAirport { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
