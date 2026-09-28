using System.ComponentModel.DataAnnotations;

namespace Relay.Core.Normality;

public sealed record NormalityOptions
{
    public const string SectionName = "Normality";

    [Range(1, 52)]
    public int BaselineWeeks { get; init; } = 8;

    [Range(1, 52)]
    public int MinimumEligibleWeeks { get; init; } = 4;

    [Range(0.1, 10.0)]
    public double BandWidth { get; init; } = 2.0;

    [Range(0.1, 10.0)]
    public double SpreadFloor { get; init; } = 1.0;
}
