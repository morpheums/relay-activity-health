using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Relay.Core.ActivityHealth;

namespace Relay.Api.Http;

public static class ActivityHealthResultExtensions
{
    public static Results<Ok<ActivityHealthReport>, ProblemHttpResult> ToHttpResult(this ActivityHealthResult result) =>
        result switch
        {
            ActivityHealthResult.Found found => TypedResults.Ok(found.Report.WithDisplayDeviations()),
            ActivityHealthResult.AccountNotFound => TypedResults.Problem(
                detail: "No account exists with this id.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Account not found"),
            ActivityHealthResult.InvalidWeek invalidWeek => TypedResults.Problem(
                detail: DetailFor(invalidWeek.Reason),
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid week"),
            _ => throw new UnreachableException($"Unhandled activity health result {result.GetType().Name}."),
        };

    private static string DetailFor(InvalidWeekReason reason) =>
        reason switch
        {
            InvalidWeekReason.NotAWeekStart => "The week must start on a Monday.",
            InvalidWeekReason.AfterLatestCompleteWeek => "The week must not be after the latest complete week.",
            InvalidWeekReason.BeforeEarliestWeek => "The week must not be before the account's earliest week.",
            _ => throw new UnreachableException($"Unhandled invalid week reason {reason}."),
        };
}
