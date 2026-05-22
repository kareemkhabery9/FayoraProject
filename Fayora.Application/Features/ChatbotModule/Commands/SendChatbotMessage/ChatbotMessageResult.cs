using System;

namespace Fayora.Application.Features.ChatbotModule.Commands.SendChatbotMessage;

public record ChatbotMessageResult(
    Guid SessionId,
    string ResponseJson
);
