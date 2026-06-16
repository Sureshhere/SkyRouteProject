using FluentAssertions;
using FluentValidation.TestHelper;
using SkyRoute.Application.DTOs.Flight;
using SkyRoute.Application.Validators;
using SkyRoute.Domain.Enums;

namespace SkyRoute.Tests.Validators;

public class FlightSearchValidatorTests
{
    private readonly FlightSearchValidator _validator = new();

    private static FlightSearchRequestDto ValidRequest() => new()
    {
        OriginAirportCode = "JFK",
        DestinationAirportCode = "LAX",
        DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
        NumberOfPassengers = 2,
        CabinClass = CabinClass.Economy
    };

    [Fact]
    public void Validate_WithValidRequest_ShouldPass()
    {
        var result = _validator.TestValidate(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenOriginEmpty_ShouldFail()
    {
        var request = ValidRequest();
        request.OriginAirportCode = "";
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.OriginAirportCode);
    }

    [Fact]
    public void Validate_WhenSameOriginAndDestination_ShouldFail()
    {
        var request = ValidRequest();
        request.DestinationAirportCode = request.OriginAirportCode;
        var result = _validator.TestValidate(request);
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void Validate_WhenDepartureDateInPast_ShouldFail()
    {
        var request = ValidRequest();
        request.DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.DepartureDate);
    }

    [Theory]
    [InlineData(0)]   // below min
    [InlineData(10)]  // above max
    public void Validate_WhenPassengersOutOfRange_ShouldFail(int passengers)
    {
        var request = ValidRequest();
        request.NumberOfPassengers = passengers;
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.NumberOfPassengers);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(9)]
    public void Validate_WhenPassengersInRange_ShouldPass(int passengers)
    {
        var request = ValidRequest();
        request.NumberOfPassengers = passengers;
        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.NumberOfPassengers);
    }
}
