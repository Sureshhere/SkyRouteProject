using Microsoft.EntityFrameworkCore;
using SkyRoute.Domain.Enums;
using SkyRoute.Domain.Models;

namespace SkyRoute.Infrastructure.Data;

public class SkyRouteDbContext : DbContext
{
    public SkyRouteDbContext(DbContextOptions<SkyRouteDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Airport> Airports => Set<Airport>();
    public DbSet<Airline> Airlines => Set<Airline>();
    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<PassengerDetail> PassengerDetails => Set<PassengerDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).IsRequired().HasMaxLength(256);
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.FullName).IsRequired().HasMaxLength(200);
        });

        // Airport
        modelBuilder.Entity<Airport>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.Code).IsUnique();
            e.Property(a => a.Code).IsRequired().HasMaxLength(10);
            e.Property(a => a.Name).IsRequired().HasMaxLength(200);
            e.Property(a => a.City).IsRequired().HasMaxLength(100);
            e.Property(a => a.Country).IsRequired().HasMaxLength(100);
            e.Property(a => a.CountryCode).IsRequired().HasMaxLength(5);
        });

        // Airline
        modelBuilder.Entity<Airline>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.Name).IsUnique();
            e.Property(a => a.Name).IsRequired().HasMaxLength(100);
            e.Property(a => a.Code).IsRequired().HasMaxLength(10);
        });

        // Flight
        modelBuilder.Entity<Flight>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.FlightNumber).IsRequired().HasMaxLength(20);
            e.Property(f => f.BaseFare).HasPrecision(18, 2);
            e.Property(f => f.CabinClass).HasConversion<string>();
            e.HasOne(f => f.Airline).WithMany(a => a.Flights).HasForeignKey(f => f.AirlineId);
            e.HasOne(f => f.OriginAirport).WithMany().HasForeignKey(f => f.OriginAirportId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(f => f.DestinationAirport).WithMany().HasForeignKey(f => f.DestinationAirportId).OnDelete(DeleteBehavior.Restrict);
        });

        // Booking
        modelBuilder.Entity<Booking>(e =>
        {
            e.HasKey(b => b.Id);
            e.HasIndex(b => b.BookingReferenceCode).IsUnique();
            e.Property(b => b.BookingReferenceCode).IsRequired().HasMaxLength(20);
            e.Property(b => b.TotalPrice).HasPrecision(18, 2);
            e.Property(b => b.PricePerPassenger).HasPrecision(18, 2);
            e.Property(b => b.Status).HasConversion<string>();
            e.HasOne(b => b.User).WithMany(u => u.Bookings).HasForeignKey(b => b.UserId);
            e.HasOne(b => b.Flight).WithMany(f => f.Bookings).HasForeignKey(b => b.FlightId).OnDelete(DeleteBehavior.Restrict);
        });

        // PassengerDetail
        modelBuilder.Entity<PassengerDetail>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.FullName).IsRequired().HasMaxLength(200);
            e.Property(p => p.Email).IsRequired().HasMaxLength(256);
            e.Property(p => p.DocumentNumber).IsRequired().HasMaxLength(50);
            e.Property(p => p.DocumentType).HasConversion<string>();
            e.Property(p => p.SeatNumber).IsRequired().HasMaxLength(5).HasDefaultValue(string.Empty);
            e.HasOne(p => p.Booking).WithMany(b => b.PassengerDetails).HasForeignKey(p => p.BookingId);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // ── Airlines ──────────────────────────────────────────────────────────
        var globalAirId  = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var budgetWingsId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        modelBuilder.Entity<Airline>().HasData(
            new Airline { Id = globalAirId,   Name = "GlobalAir",   Code = "GA", IsActive = true },
            new Airline { Id = budgetWingsId, Name = "BudgetWings", Code = "BW", IsActive = true }
        );

        // ── Airports (6 airports, 3 countries: US, GB, IN) ───────────────────
        var jfkId = Guid.Parse("aa000001-0000-0000-0000-000000000001");
        var laxId = Guid.Parse("aa000001-0000-0000-0000-000000000002");
        var ordId = Guid.Parse("aa000001-0000-0000-0000-000000000003");
        var lhrId = Guid.Parse("aa000001-0000-0000-0000-000000000004");
        var bomId = Guid.Parse("aa000001-0000-0000-0000-000000000005");
        var delId = Guid.Parse("aa000001-0000-0000-0000-000000000006");

        modelBuilder.Entity<Airport>().HasData(
            new Airport { Id = jfkId, Code = "JFK", Name = "John F. Kennedy International Airport",             City = "New York",    Country = "United States",  CountryCode = "US", IsActive = true },
            new Airport { Id = laxId, Code = "LAX", Name = "Los Angeles International Airport",                 City = "Los Angeles", Country = "United States",  CountryCode = "US", IsActive = true },
            new Airport { Id = ordId, Code = "ORD", Name = "O'Hare International Airport",                      City = "Chicago",     Country = "United States",  CountryCode = "US", IsActive = true },
            new Airport { Id = lhrId, Code = "LHR", Name = "Heathrow Airport",                                  City = "London",      Country = "United Kingdom", CountryCode = "GB", IsActive = true },
            new Airport { Id = bomId, Code = "BOM", Name = "Chhatrapati Shivaji Maharaj International Airport", City = "Mumbai",      Country = "India",          CountryCode = "IN", IsActive = true },
            new Airport { Id = delId, Code = "DEL", Name = "Indira Gandhi International Airport",               City = "New Delhi",   Country = "India",          CountryCode = "IN", IsActive = true }
        );

        // ── Flights (predefined dataset per route, 2 airlines × 4 departures × 3 cabins) ─
        var routes = new (Guid OriginId, Guid DestId, int DurationMinutes)[]
        {
            (jfkId, laxId, 330), (laxId, jfkId, 315),  // US domestic
            (jfkId, ordId, 150), (ordId, jfkId, 145),  // US domestic
            (bomId, delId, 130), (delId, bomId, 125),  // IN domestic
            (jfkId, lhrId, 420), (lhrId, jfkId, 450),  // US ↔ UK international
            (jfkId, bomId, 870), (bomId, jfkId, 900),  // US ↔ IN international
            (lhrId, delId, 540), (delId, lhrId, 570),  // GB ↔ IN international
        };

        var cabinBaseFares = new Dictionary<CabinClass, decimal>
        {
            { CabinClass.Economy,    150m },
            { CabinClass.Business,   450m },
            { CabinClass.FirstClass, 900m }
        };

        var schedule = new (Guid AirlineId, string Code, TimeSpan DepartureTime)[]
        {
            (globalAirId,   "GA", new TimeSpan(6,  0, 0)),
            (budgetWingsId, "BW", new TimeSpan(10, 30, 0)),
            (globalAirId,   "GA", new TimeSpan(14, 0, 0)),
            (budgetWingsId, "BW", new TimeSpan(18, 45, 0)),
        };

        var flights = new List<Flight>();
        int counter = 1;

        foreach (var route in routes)
        {
            foreach (var cabin in cabinBaseFares.Keys)
            {
                foreach (var slot in schedule)
                {
                    // Generate a deterministic GUID from the counter
                    var flightId = new Guid(counter, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                    var arrivalTime = slot.DepartureTime.Add(TimeSpan.FromMinutes(route.DurationMinutes));

                    flights.Add(new Flight
                    {
                        Id               = flightId,
                        AirlineId        = slot.AirlineId,
                        FlightNumber     = $"{slot.Code}{counter:D3}",
                        OriginAirportId      = route.OriginId,
                        DestinationAirportId = route.DestId,
                        DepartureTime    = slot.DepartureTime,
                        ArrivalTime      = arrivalTime,
                        DurationMinutes  = route.DurationMinutes,
                        CabinClass       = cabin,
                        BaseFare         = cabinBaseFares[cabin],
                        IsActive         = true
                    });
                    counter++;
                }
            }
        }

        modelBuilder.Entity<Flight>().HasData(flights);
    }
}
