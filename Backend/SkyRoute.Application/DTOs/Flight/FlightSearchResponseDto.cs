namespace SkyRoute.Application.DTOs.Flight;

public class FlightSearchResponseDto
{
    public IEnumerable<FlightResultDto> Flights { get; set; } = new List<FlightResultDto>();
    public SearchMetadataDto SearchMetadata { get; set; } = new();
}

public class SearchMetadataDto
{
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateOnly DepartureDate { get; set; }
    public int Passengers { get; set; }
    public string CabinClass { get; set; } = string.Empty;
    public int ResultsCount { get; set; }
}
