using System.Text.Json;

namespace Relay.Api.Tests.Fixtures;

public sealed record SeriesJson(
    string? Location,
    int Count,
    int WeeksUsed,
    double? Median,
    int? Low,
    int? High,
    string Status,
    double? Deviation)
{
    public const double Tolerance = 1e-9;

    public static SeriesJson From(JsonElement series)
    {
        var baseline = series.GetProperty("baseline");
        return new SeriesJson(
            series.TryGetProperty("location", out var location) ? location.GetString() : null,
            series.GetProperty("count").GetInt32(),
            baseline.GetProperty("weeksUsed").GetInt32(),
            NullableDouble(baseline.GetProperty("median")),
            NullableInt(baseline.GetProperty("low")),
            NullableInt(baseline.GetProperty("high")),
            series.GetProperty("status").GetString() ?? string.Empty,
            NullableDouble(series.GetProperty("deviation")));
    }

    public void ShouldHaveRange(int count, int low, int high, string status)
    {
        Count.ShouldBe(count, Location);
        Low.ShouldBe(low, Location);
        High.ShouldBe(high, Location);
        Status.ShouldBe(status, Location);
    }

    public void ShouldHaveMedian(double median)
    {
        Median.ShouldNotBeNull(Location);
        Median.Value.ShouldBe(median, Tolerance, Location);
    }

    public void ShouldHaveDeviation(double deviation)
    {
        Deviation.ShouldNotBeNull(Location);
        Deviation.Value.ShouldBe(deviation, Tolerance, Location);
    }

    public void ShouldBeInsufficient(int count, int weeksUsed)
    {
        Count.ShouldBe(count, Location);
        Status.ShouldBe(WireStatus.InsufficientData, Location);
        WeeksUsed.ShouldBe(weeksUsed, Location);
        Median.ShouldBeNull(Location);
        Low.ShouldBeNull(Location);
        High.ShouldBeNull(Location);
        Deviation.ShouldBeNull(Location);
    }

    private static double? NullableDouble(JsonElement value) =>
        value.ValueKind == JsonValueKind.Null ? null : value.GetDouble();

    private static int? NullableInt(JsonElement value) =>
        value.ValueKind == JsonValueKind.Null ? null : value.GetInt32();
}
