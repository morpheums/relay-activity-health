using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Relay.Infrastructure.Persistence;

public sealed class ActivityEventConfiguration : IEntityTypeConfiguration<ActivityEvent>
{
    public const string AccountOccurredIndexName = "IX_activity_events_account_occurred";

    public void Configure(EntityTypeBuilder<ActivityEvent> builder)
    {
        builder.ToTable("activity_events");
        builder.HasKey(activityEvent => activityEvent.Id);

        builder.Property(activityEvent => activityEvent.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(activityEvent => activityEvent.AccountId).HasColumnName("account_id");
        builder.Property(activityEvent => activityEvent.Location).HasColumnName("location").HasColumnType("varchar(80)").HasMaxLength(80).IsUnicode(false);
        builder.Property(activityEvent => activityEvent.EventType).HasColumnName("event_type").HasColumnType("varchar(40)").HasMaxLength(40).IsUnicode(false);
        builder.Property(activityEvent => activityEvent.OccurredAt).HasColumnName("occurred_at");
        builder.Property(activityEvent => activityEvent.DurationSeconds).HasColumnName("duration_seconds");
        builder.Property(activityEvent => activityEvent.Outcome).HasColumnName("outcome").HasColumnType("varchar(40)").HasMaxLength(40).IsUnicode(false);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(activityEvent => activityEvent.AccountId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(activityEvent => new { activityEvent.AccountId, activityEvent.OccurredAt }, AccountOccurredIndexName)
            .IncludeProperties(activityEvent => new
            {
                activityEvent.Location,
                activityEvent.EventType,
                activityEvent.DurationSeconds,
                activityEvent.Outcome,
            });
    }
}
