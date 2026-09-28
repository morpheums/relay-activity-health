using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Relay.Api.Http;
using Relay.Core.ActivityHealth;
using static Relay.Api.Tests.Fixtures.ReportBuilder;

namespace Relay.Api.Tests.ActivityHealth;

public sealed class ActivityHealthResultMappingTests
{
    [Fact]
    public void ToHttpResultFoundReturnsOkWithTheReportForTheAccount()
    {
        var report = Report(SummaryWithDeviation(0.5), LocationWithDeviation("Site A", -1.25));

        var httpResult = new ActivityHealthResult.Found(report).ToHttpResult();

        var ok = httpResult.Result.ShouldBeOfType<Ok<ActivityHealthReport>>();
        ok.StatusCode.ShouldBe(StatusCodes.Status200OK);
        ok.Value.ShouldNotBeNull().Account.ShouldBe(report.Account);
    }

    [Fact]
    public void ToHttpResultAccountNotFoundReturnsNotFoundProblem()
    {
        var httpResult = new ActivityHealthResult.AccountNotFound().ToHttpResult();

        var problem = httpResult.Result.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        problem.ProblemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Theory]
    [InlineData(InvalidWeekReason.NotAWeekStart)]
    [InlineData(InvalidWeekReason.AfterLatestCompleteWeek)]
    [InlineData(InvalidWeekReason.BeforeEarliestWeek)]
    public void ToHttpResultInvalidWeekReturnsBadRequestProblem(InvalidWeekReason reason)
    {
        var httpResult = new ActivityHealthResult.InvalidWeek(reason).ToHttpResult();

        var problem = httpResult.Result.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        problem.ProblemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
    }
}
