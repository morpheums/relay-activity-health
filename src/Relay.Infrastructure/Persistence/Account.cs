namespace Relay.Infrastructure.Persistence;

public sealed class Account
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Industry { get; set; }

    public required string Timezone { get; set; }

    public DateTime CreatedAt { get; set; }
}
