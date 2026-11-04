namespace SkyRoute.Application.Interfaces;

public interface ISkyRouteMetrics
{
    // Flight Search metrics
    void RecordFlightSearchAttempt();
    void RecordFlightSearchOutcome(string outcome);

    // Flight search duration metrics
    void RecordFlightSearchDuration(double durationSeconds);

    // Booking duration metrics
    void RecordBookingDuration(double durationSeconds);

    // Booking metrics
    void RecordBookingAttempt();
    void RecordBookingOutcome(string outcome);

    // Pricing metrics
    void RecordPricingStrategyUsage(string providerName);

}