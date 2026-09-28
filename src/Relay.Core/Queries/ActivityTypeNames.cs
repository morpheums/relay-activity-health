using System.Diagnostics.CodeAnalysis;

namespace Relay.Core.Queries;

public static class ActivityTypeNames
{
    public const string All = "all";
    public const string CallReceived = "call_received";
    public const string LeadCreated = "lead_created";
    public const string AppointmentSet = "appointment_set";

    public const string ExactMatchPattern = $"^({All}|{CallReceived}|{LeadCreated}|{AppointmentSet})$";

    public static string ToName(this ActivityType activityType) =>
        throw new NotImplementedException();

    public static bool TryParse([NotNullWhen(true)] string? name, out ActivityType activityType) =>
        throw new NotImplementedException();
}
