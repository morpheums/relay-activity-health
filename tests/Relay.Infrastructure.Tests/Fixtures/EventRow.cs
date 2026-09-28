namespace Relay.Infrastructure.Tests.Fixtures;

public sealed record EventRow(
    int AccountId,
    string Location,
    string EventType,
    DateTime OccurredAtUtc,
    int? DurationSeconds,
    string? Outcome)
{
    public const string CallReceived = "call_received";
    public const string LeadCreated = "lead_created";
    public const string AppointmentSet = "appointment_set";

    public const int DefaultAccountId = 1;

    public static EventRow EventAt(string occurredAtUtc) =>
        new(DefaultAccountId, "Site A", CallReceived, Utc.At(occurredAtUtc), 120, "connected");
}
