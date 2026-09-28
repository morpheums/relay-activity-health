namespace Relay.Infrastructure.Tests.Fixtures;

public sealed record AccountRow(int Id, string Name, string Timezone)
{
    public string Industry { get; init; } = "home_services";

    public DateTime CreatedAtUtc { get; init; } = Utc.At("2025-06-01T00:00:00Z");
}
