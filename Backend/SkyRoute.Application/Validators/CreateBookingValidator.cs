using FluentValidation;
using SkyRoute.Application.DTOs.Booking;

namespace SkyRoute.Application.Validators;

public class CreateBookingValidator : AbstractValidator<CreateBookingRequestDto>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.FlightId)
            .NotEmpty().WithMessage("Flight ID is required.");

        RuleFor(x => x.DepartureDate)
            .Must(date => date >= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Departure date must be today or in the future.");

        RuleFor(x => x.Passengers)
            .NotEmpty().WithMessage("At least one passenger is required.")
            .Must(p => p.Count <= 9).WithMessage("Maximum 9 passengers allowed.");

        RuleForEach(x => x.Passengers).SetValidator(new PassengerInputValidator());
    }
}

public class PassengerInputValidator : AbstractValidator<PassengerInputDto>
{
    public PassengerInputValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Passenger full name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Passenger email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256);

        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("Document number is required.")
            .MaximumLength(50);
    }
}
