namespace Relay.Core.Normality;

public sealed record LocationHealth(string Location, int Count, Baseline Baseline, HealthStatus Status, double? Deviation)
    : SeriesHealth(Count, Baseline, Status, Deviation);
