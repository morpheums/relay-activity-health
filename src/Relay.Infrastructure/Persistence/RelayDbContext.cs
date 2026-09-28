using Microsoft.EntityFrameworkCore;

namespace Relay.Infrastructure.Persistence;

public sealed class RelayDbContext(DbContextOptions<RelayDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<ActivityEvent> ActivityEvents => Set<ActivityEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RelayDbContext).Assembly);

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        // Scalar SqlQuery<DateTime> results bypass property conventions and use the default type mapping.
        configurationBuilder.DefaultTypeMapping<DateTime>().HasConversion<UtcDateTimeConverter>();
    }
}
