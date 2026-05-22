using Fayora.Domain.Entities.NotificationModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fayora.Infrastructure.Persistence.Configurations.NotificationModule;

public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.ToTable("DeviceTokens");

        builder.HasKey(dt => dt.Id);

        builder.Property(dt => dt.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(dt => dt.DeviceType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(dt => dt.LastActiveAt)
            .IsRequired();

        builder.HasIndex(dt => dt.Token)
            .IsUnique();

        builder.HasIndex(dt => dt.UserId);
    }
}
