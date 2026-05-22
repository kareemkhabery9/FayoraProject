using System;

namespace Fayora.Contracts.ChatbotModule;

public record SendChatbotMessageRequest(
    string Content,
    Guid? SessionId = null
);
