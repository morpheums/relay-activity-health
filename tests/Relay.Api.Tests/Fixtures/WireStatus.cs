namespace Relay.Api.Tests.Fixtures;

public static class WireStatus
{
    public const string Above = "above";
    public const string Below = "below";
    public const string Normal = "normal";
    public const string InsufficientData = "insufficient_data";

    public static IReadOnlyList<string> All { get; } = [Above, Below, Normal, InsufficientData];

    public static IReadOnlyList<string> Flagged { get; } = [Above, Below];
}
