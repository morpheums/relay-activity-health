namespace Relay.Core.Normality;

public sealed record BaselineAssessment(
    int WeeksUsed,
    double? Median,
    int? Low,
    int? High,
    HealthStatus Status,
    double? Deviation);
