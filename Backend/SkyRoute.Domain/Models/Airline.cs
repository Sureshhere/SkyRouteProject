namespace SkyRoute.Domain.Models;

public class Airline
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
