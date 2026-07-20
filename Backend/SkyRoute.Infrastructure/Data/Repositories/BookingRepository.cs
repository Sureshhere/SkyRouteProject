using Microsoft.EntityFrameworkCore;
using SkyRoute.Application.Interfaces;
using SkyRoute.Domain.Enums;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Data;

namespace SkyRoute.Infrastructure.Data.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly SkyRouteDbContext _context;

    public BookingRepository(SkyRouteDbContext context) => _context = context;

    public async Task<Booking?> GetByReferenceCodeAsync(string referenceCode) =>
        await _context.Bookings
            .Include(b => b.Flight).ThenInclude(f => f.Airline)
            .Include(b => b.Flight).ThenInclude(f => f.OriginAirport)
            .Include(b => b.Flight).ThenInclude(f => f.DestinationAirport)
            .Include(b => b.PassengerDetails)
            .FirstOrDefaultAsync(b => b.BookingReferenceCode == referenceCode);

    public async Task<Booking?> GetByIdAsync(Guid id) =>
        await _context.Bookings
            .Include(b => b.Flight).ThenInclude(f => f.Airline)
            .Include(b => b.Flight).ThenInclude(f => f.OriginAirport)
            .Include(b => b.Flight).ThenInclude(f => f.DestinationAirport)
            .Include(b => b.PassengerDetails)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<Booking> AddAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
        return booking;
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<IReadOnlyList<string>> GetOccupiedSeatsAsync(Guid flightId, DateOnly departureDate)
    {
        var dateStart = departureDate.ToDateTime(TimeOnly.MinValue);
        var dateEnd = dateStart.AddDays(1);
        return await _context.Bookings
            .Where(b => b.FlightId == flightId
                     && b.DepartureDate >= dateStart
                     && b.DepartureDate < dateEnd
                     && b.Status == BookingStatus.Confirmed)
            .SelectMany(b => b.PassengerDetails)
            .Where(p => p.SeatNumber != string.Empty)
            .Select(p => p.SeatNumber)
            .Distinct()
            .ToListAsync();
    }
}
