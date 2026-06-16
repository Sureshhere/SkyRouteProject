namespace SkyRoute.Application.DTOs.Flight;

public class FlightResultDto
{
    public Guid Id { get; set; }
    public string AirlineName { get; set; } = string.Empty;
    public string AirlineCode { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string OriginCode { get; set; } = string.Empty;
    public string DestinationCode { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public int DurationMinutes { get; set; }
    public string CabinClass { get; set; } = string.Empty;
    public PricingDto Pricing { get; set; } = new();
}

public class PricingDto
{
    public decimal BaseFare { get; set; }
    public decimal PricePerPassenger { get; set; }
    public decimal TotalPrice { get; set; }
    public string PricingRule { get; set; } = string.Empty;
}
