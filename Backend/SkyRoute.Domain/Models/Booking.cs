using SkyRoute.Domain.Enums;

namespace SkyRoute.Domain.Models;

public class Booking
{
    public Guid Id { get; set; }
    public string BookingReferenceCode { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid FlightId { get; set; }
    public DateTime DepartureDate { get; set; }
    public int NumberOfPassengers { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal PricePerPassenger { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Flight Flight { get; set; } = null!;
    public ICollection<PassengerDetail> PassengerDetails { get; set; } = new List<PassengerDetail>();
}
