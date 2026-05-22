using System;
using Fayora.Domain.Common.Entity;

namespace Fayora.Domain.Entities.ChatbotModule;

public class ChatbotMessage : AuditableEntity<long>
{
    public Guid SessionId { get; private set; }
    public string Role { get; private set; } = null!; // "user" or "model"
    public string Content { get; private set; } = null!;

    private ChatbotMessage() { }

    public static ChatbotMessage Create(Guid sessionId, string role, string content)
    {
        return new ChatbotMessage
        {
            SessionId = sessionId,
            Role = role,
            Content = content
        };
    }
}
