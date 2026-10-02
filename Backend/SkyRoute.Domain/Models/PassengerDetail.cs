using SkyRoute.Domain.Enums;

namespace SkyRoute.Domain.Models;

public class PassengerDetail
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public int PassengerIndex { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Booking Booking { get; set; } = null!;
}
