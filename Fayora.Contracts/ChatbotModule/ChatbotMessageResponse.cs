using System;

namespace Fayora.Contracts.ChatbotModule;

public record ChatbotMessageResponse(
    long Id,
    string Role,
    string Content,
    DateTimeOffset CreatedAt
);
