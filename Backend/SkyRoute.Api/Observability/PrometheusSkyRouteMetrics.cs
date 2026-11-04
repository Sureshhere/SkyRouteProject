using System.Diagnostics.Metrics;
using Prometheus;
using SkyRoute.Application.Interfaces;

namespace SkyRoute.Api.Observability;

public sealed class PrometheusSkyRouteMetrics : ISkyRouteMetrics
{
    private static readonly Counter FlightSearchAttempts = Metrics.CreateCounter(
        "skyroute_flight_search_attempts_total",
        "Total number of flight search attempts."
    );

    private static readonly Counter FlightSearchOutcomes = Metrics.CreateCounter(
        "skyroute_flight_search_outcomes_total",
        "Total number of flight search outcomes.",
        new CounterConfiguration
        {
            LabelNames = new[] { "outcome" }
        }
    );

    private static readonly Counter BookingAttempts = Metrics.CreateCounter(
        "skyroute_booking_attempts_total",
        "Total number of booking attempts."
    );

    private static readonly Counter BookingOutcomes = Metrics.CreateCounter(
        "skyroute_booking_outcomes_total",
        "Total number of booking outcomes.",
        new CounterConfiguration
        {
            LabelNames = new[] { "outcome" }
        }
    );

    private static readonly Counter PricingStrategyUsage = Metrics.CreateCounter(
        "skyroute_pricing_strategy_usage_total",
        "Number of pricing calculations by provider.",
        new CounterConfiguration
        {
            LabelNames = new[] { "provider" }
        }
    );

    private static readonly Histogram FlightSearchDuration = Metrics.CreateHistogram(
        "skyroute_flight_search_duration_seconds",
        "Duration of flight search operations in seconds.",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(start: 0.01, factor: 2, count: 12)
        }
    );

    private static readonly Histogram BookingDuration = Metrics.CreateHistogram(
        "skyroute_booking_duration_seconds",
        "Duration of booking operations in seconds.",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(start: 0.01, factor: 2, count: 12)
        }
    );

    public void RecordFlightSearchAttempt()
    {
        FlightSearchAttempts.Inc();
    }

    public void RecordFlightSearchOutcome(string outcome)
    {
        FlightSearchOutcomes.WithLabels(outcome).Inc();
    }

    public void RecordBookingAttempt()
    {
        BookingAttempts.Inc();
    }

    public void RecordBookingOutcome(string outcome)
    {
        BookingOutcomes.WithLabels(outcome).Inc();
    }

    public void RecordPricingStrategyUsage(string provider)
    {
        PricingStrategyUsage.WithLabels(provider).Inc();
    }

    public void RecordFlightSearchDuration(double durationSeconds)
    {
        FlightSearchDuration.Observe(durationSeconds);
    }

    public void RecordBookingDuration(double durationSeconds)
    {
        BookingDuration.Observe(durationSeconds);
    }
}