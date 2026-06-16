using SkyRoute.Application.DTOs.Booking;

namespace SkyRoute.Application.Interfaces;

public interface IBookingService
{
    Task<BookingConfirmationDto> CreateBookingAsync(CreateBookingRequestDto request, Guid userId);
    Task<BookingConfirmationDto> GetBookingByReferenceAsync(string referenceCode, Guid userId);
}
