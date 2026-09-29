using Relay.Core.Normality;

namespace Relay.Core.Tests.Normality;

public sealed class LocationRankerTests
{
    private static readonly Baseline SufficientBaseline = new(8, 5.0, 2, 10);

    private readonly LocationRanker _locationRanker = new();

    private static LocationHealth Evaluated(string location, HealthStatus status, double deviation) =>
        new(location, 5, SufficientBaseline, status, deviation);

    private static LocationHealth Insufficient(string location, int weeksUsed) =>
        new(location, 5, new Baseline(weeksUsed, null, null, null), HealthStatus.InsufficientData, null);

    private List<string> RankedNames(params LocationHealth[] locations) =>
        _locationRanker.Rank(locations).Select(location => location.Location).ToList();

    [Fact]
    public void RankInsufficientDataRowsGoLastSortedByName()
    {
        var rankedNames = RankedNames(
            Insufficient("Site C", 3),
            Evaluated("Site Z", HealthStatus.Normal, 0.0),
            Insufficient("Site A", 0),
            Evaluated("Site Y", HealthStatus.Above, 2.5));

        rankedNames.ShouldBe(["Site Y", "Site Z", "Site A", "Site C"]);
    }

    [Fact]
    public void RankFlaggedRowsGoBeforeNormalRowsWithLargerDeviation()
    {
        var rankedNames = RankedNames(
            Evaluated("Site A", HealthStatus.Normal, -1.86),
            Evaluated("Site B", HealthStatus.Above, 1.83));

        rankedNames.ShouldBe(["Site B", "Site A"]);
    }

    [Fact]
    public void RankAboveAndBelowRowsAreOrderedTogetherByAbsoluteDeviationDescending()
    {
        var rankedNames = RankedNames(
            Evaluated("Site A", HealthStatus.Above, 2.11),
            Evaluated("Site B", HealthStatus.Below, -3.19),
            Evaluated("Site C", HealthStatus.Above, 2.81));

        rankedNames.ShouldBe(["Site B", "Site C", "Site A"]);
    }

    [Fact]
    public void RankUsesUnroundedDeviationWhenRoundedValuesTie()
    {
        var rankedNames = RankedNames(
            Evaluated("Site A", HealthStatus.Normal, 1.296),
            Evaluated("Site B", HealthStatus.Normal, 1.304));

        rankedNames.ShouldBe(["Site B", "Site A"]);
    }

    [Fact]
    public void RankEqualAbsoluteDeviationPutsBelowBeforeAboveThenName()
    {
        var rankedNames = RankedNames(
            Evaluated("Site B", HealthStatus.Above, 2.5),
            Evaluated("Site A", HealthStatus.Above, 2.5),
            Evaluated("Site C", HealthStatus.Below, -2.5));

        rankedNames.ShouldBe(["Site C", "Site A", "Site B"]);
    }

    [Fact]
    public void RankNormalRowsWithOppositeSignsAndEqualMagnitudeAreOrderedByName()
    {
        var rankedNames = RankedNames(
            Evaluated("Site B", HealthStatus.Normal, -0.5),
            Evaluated("Site A", HealthStatus.Normal, 0.5));

        rankedNames.ShouldBe(["Site A", "Site B"]);
    }

    [Fact]
    public void RankNameTieBreakIsOrdinalSoUppercaseSortsBeforeLowercase()
    {
        var rankedNames = RankedNames(
            Evaluated("site a", HealthStatus.Normal, 0.0),
            Evaluated("Site B", HealthStatus.Normal, 0.0));

        rankedNames.ShouldBe(["Site B", "site a"]);
    }

    [Fact]
    public void RankIsIndependentOfInputOrder()
    {
        var locations = new[]
        {
            Evaluated("Site A", HealthStatus.Normal, 0.0),
            Evaluated("Site B", HealthStatus.Normal, 0.0),
            Evaluated("Site C", HealthStatus.Below, -2.5),
            Evaluated("Site D", HealthStatus.Above, 2.5),
            Insufficient("Site E", 1),
            Insufficient("Site F", 1),
        };

        var rankedForward = RankedNames(locations);
        var rankedReversed = RankedNames(locations.Reverse().ToArray());

        rankedForward.ShouldBe(["Site C", "Site D", "Site A", "Site B", "Site E", "Site F"]);
        rankedReversed.ShouldBe(rankedForward);
    }
}
