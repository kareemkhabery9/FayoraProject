using Fayora.Domain.Entities.ChatbotModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fayora.Infrastructure.Persistence.Configurations.ChatbotModule;

public class ChatbotSessionConfiguration : IEntityTypeConfiguration<ChatbotSession>
{
    public void Configure(EntityTypeBuilder<ChatbotSession> builder)
    {
        builder.ToTable("ChatbotSessions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.DeviceId)
               .IsRequired()
               .HasMaxLength(256);

        builder.HasIndex(s => s.DeviceId);
        builder.HasIndex(s => s.UserId);

        builder.HasMany(s => s.Messages)
               .WithOne()
               .HasForeignKey(m => m.SessionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
