using System.Globalization;
using Relay.Core.Queries;

namespace Relay.Api.Http;

public static class ActivityHealthRequestExtensions
{
    public static DateOnly? ParsedWeek(this ActivityHealthRequest request) =>
        request.Week is null
            ? null
            : DateOnly.ParseExact(request.Week, IsoDateAttribute.Format, CultureInfo.InvariantCulture);

    public static ActivityType ParsedEventType(this ActivityHealthRequest request) =>
        request.Type is not null && ActivityTypeNames.TryParse(request.Type, out var eventType)
            ? eventType
            : ActivityType.All;
}
