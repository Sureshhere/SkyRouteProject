using FluentValidation;
using SkyRoute.Application.DTOs.Flight;

namespace SkyRoute.Application.Validators;

public class FlightSearchValidator : AbstractValidator<FlightSearchRequestDto>
{
    public FlightSearchValidator()
    {
        RuleFor(x => x.OriginAirportCode)
            .NotEmpty().WithMessage("Origin airport is required.")
            .MaximumLength(10);

        RuleFor(x => x.DestinationAirportCode)
            .NotEmpty().WithMessage("Destination airport is required.")
            .MaximumLength(10);

        RuleFor(x => x)
            .Must(x => x.OriginAirportCode != x.DestinationAirportCode)
            .WithMessage("Origin and destination airports must be different.");

        RuleFor(x => x.DepartureDate)
            .Must(date => date >= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Departure date must be today or in the future.");

        RuleFor(x => x.NumberOfPassengers)
            .InclusiveBetween(1, 9)
            .WithMessage("Number of passengers must be between 1 and 9.");

        RuleFor(x => x.CabinClass)
            .IsInEnum()
            .WithMessage("Invalid cabin class. Valid options: Economy, Business, FirstClass.");
    }
}
