using Fayora.Domain.Entities.ChatbotModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fayora.Infrastructure.Persistence.Configurations.ChatbotModule;

public class ChatbotMessageConfiguration : IEntityTypeConfiguration<ChatbotMessage>
{
    public void Configure(EntityTypeBuilder<ChatbotMessage> builder)
    {
        builder.ToTable("ChatbotMessages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Role)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(m => m.Content)
               .IsRequired();

        builder.HasIndex(m => m.SessionId);
    }
}
