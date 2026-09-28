namespace Relay.Core.ActivityHealth;

public sealed record ActivityHealthQuery(int AccountId, DateOnly? Week, ActivityType EventType);
