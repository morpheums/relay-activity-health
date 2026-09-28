using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Relay.Infrastructure.Persistence;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        builder.HasKey(account => account.Id);

        builder.Property(account => account.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(account => account.Name).HasColumnName("name").HasColumnType("varchar(120)").HasMaxLength(120).IsUnicode(false);
        builder.Property(account => account.Industry).HasColumnName("industry").HasColumnType("varchar(60)").HasMaxLength(60).IsUnicode(false);
        builder.Property(account => account.Timezone).HasColumnName("timezone").HasColumnType("varchar(60)").HasMaxLength(60).IsUnicode(false);
        builder.Property(account => account.CreatedAt).HasColumnName("created_at");
    }
}
