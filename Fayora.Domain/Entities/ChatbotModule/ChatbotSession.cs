using System;
using System.Collections.Generic;
using Fayora.Domain.Common.Entity;

namespace Fayora.Domain.Entities.ChatbotModule;

public class ChatbotSession : AuditableEntity<Guid>
{
    public string DeviceId { get; private set; } = null!;
    public Guid? UserId { get; private set; }

    private readonly List<ChatbotMessage> _messages = [];
    public IReadOnlyCollection<ChatbotMessage> Messages => _messages.AsReadOnly();

    private ChatbotSession() { }

    public static ChatbotSession Create(string deviceId, Guid? userId = null)
    {
        return new ChatbotSession
        {
            Id = Guid.NewGuid(),
            DeviceId = deviceId,
            UserId = userId
        };
    }

    public void UpdateLastMessageAt()
    {
        Updated();
    }
}
