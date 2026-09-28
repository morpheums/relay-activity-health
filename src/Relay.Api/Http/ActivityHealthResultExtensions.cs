using Microsoft.AspNetCore.Http.HttpResults;
using Relay.Core.ActivityHealth;

namespace Relay.Api.Http;

public static class ActivityHealthResultExtensions
{
    public static Results<Ok<ActivityHealthReport>, ProblemHttpResult> ToHttpResult(this ActivityHealthResult result) =>
        throw new NotImplementedException();
}
