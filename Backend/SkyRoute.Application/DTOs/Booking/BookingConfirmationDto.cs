namespace SkyRoute.Application.DTOs.Booking;

public class BookingConfirmationDto
{
    public Guid BookingId { get; set; }
    public string BookingReferenceCode { get; set; } = string.Empty;
    public BookingFlightSummaryDto FlightDetails { get; set; } = new();
    public BookingPricingDto Pricing { get; set; } = new();
    public IEnumerable<PassengerSummaryDto> Passengers { get; set; } = new List<PassengerSummaryDto>();
    public string BookingStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class BookingFlightSummaryDto
{
    public string AirlineName { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string CabinClass { get; set; } = string.Empty;
}

public class BookingPricingDto
{
    public decimal TotalPrice { get; set; }
    public decimal PricePerPassenger { get; set; }
    public int NumberOfPassengers { get; set; }
}

public class PassengerSummaryDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
}
