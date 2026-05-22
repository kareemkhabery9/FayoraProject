using Fayora.Domain.Entities.NotificationModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fayora.Infrastructure.Persistence.Configurations.NotificationModule;

public class PushCampaignConfiguration : IEntityTypeConfiguration<PushCampaign>
{
    public void Configure(EntityTypeBuilder<PushCampaign> builder)
    {
        builder.ToTable("PushCampaigns");

        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(pc => pc.Body)
            .IsRequired();

        builder.Property(pc => pc.ImageUrl)
            .HasMaxLength(1024);

        builder.Property(pc => pc.TargetAudience)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pc => pc.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pc => pc.HangfireJobId)
            .HasMaxLength(100);

        builder.Property(pc => pc.SuccessCount)
            .HasDefaultValue(0);

        builder.Property(pc => pc.FailureCount)
            .HasDefaultValue(0);
    }
}
