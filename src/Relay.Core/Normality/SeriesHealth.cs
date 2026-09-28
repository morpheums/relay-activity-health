namespace Relay.Core.Normality;

public record SeriesHealth(int Count, Baseline Baseline, HealthStatus Status, double? Deviation);
