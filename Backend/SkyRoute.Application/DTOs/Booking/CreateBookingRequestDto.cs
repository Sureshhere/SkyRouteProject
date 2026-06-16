using System.ComponentModel.DataAnnotations;

namespace SkyRoute.Application.DTOs.Booking;

public class CreateBookingRequestDto
{
    [Required]
    public Guid FlightId { get; set; }

    [Required]
    public DateOnly DepartureDate { get; set; }

    [Required]
    [MinLength(1)]
    public List<PassengerInputDto> Passengers { get; set; } = new();
}

public class PassengerInputDto
{
    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string DocumentNumber { get; set; } = string.Empty;
}
