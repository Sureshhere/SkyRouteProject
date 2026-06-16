using SkyRoute.Application.Common;
using SkyRoute.Application.DTOs.Flight;
using SkyRoute.Application.Interfaces;

namespace SkyRoute.Application.Services;

public class FlightSearchService : IFlightSearchService
{
    private readonly IFlightRepository _flightRepository;
    private readonly IAirportRepository _airportRepository;
    private readonly IEnumerable<IFlightPricingStrategy> _pricingStrategies;

    public FlightSearchService(
        IFlightRepository flightRepository,
        IAirportRepository airportRepository,
        IEnumerable<IFlightPricingStrategy> pricingStrategies)
    {
        _flightRepository = flightRepository;
        _airportRepository = airportRepository;
        _pricingStrategies = pricingStrategies;
    }

    public async Task<FlightSearchResponseDto> SearchFlightsAsync(FlightSearchRequestDto request)
    {
        var origin = await _airportRepository.GetByCodeAsync(request.OriginAirportCode.ToUpperInvariant())
            ?? throw new AppException($"Origin airport '{request.OriginAirportCode}' not found.", 404);

        var destination = await _airportRepository.GetByCodeAsync(request.DestinationAirportCode.ToUpperInvariant())
            ?? throw new AppException($"Destination airport '{request.DestinationAirportCode}' not found.", 404);

        var flights = await _flightRepository.GetByRouteAndCabinAsync(origin.Id, destination.Id, request.CabinClass);

        var results = new List<FlightResultDto>();

        foreach (var flight in flights)
        {
            var strategy = _pricingStrategies.FirstOrDefault(s => s.ProviderName == flight.Airline.Name);
            if (strategy == null) continue;

            var pricing = strategy.CalculatePrice(flight.BaseFare, request.NumberOfPassengers);

            var departureDateTime = request.DepartureDate.ToDateTime(TimeOnly.FromTimeSpan(flight.DepartureTime));
            var arrivalDateTime = departureDateTime.AddMinutes(flight.DurationMinutes);

            results.Add(new FlightResultDto
            {
                Id = flight.Id,
                AirlineName = flight.Airline.Name,
                AirlineCode = flight.Airline.Code,
                FlightNumber = flight.FlightNumber,
                OriginCode = origin.Code,
                DestinationCode = destination.Code,
                DepartureTime = departureDateTime,
                ArrivalTime = arrivalDateTime,
                DurationMinutes = flight.DurationMinutes,
                CabinClass = flight.CabinClass.ToString(),
                Pricing = new PricingDto
                {
                    BaseFare = pricing.BaseFare,
                    PricePerPassenger = pricing.PricePerPassenger,
                    TotalPrice = pricing.TotalPrice,
                    PricingRule = pricing.PricingRule
                }
            });
        }

        return new FlightSearchResponseDto
        {
            Flights = results,
            SearchMetadata = new SearchMetadataDto
            {
                Origin = origin.Code,
                Destination = destination.Code,
                DepartureDate = request.DepartureDate,
                Passengers = request.NumberOfPassengers,
                CabinClass = request.CabinClass.ToString(),
                ResultsCount = results.Count
            }
        };
    }
}
