using System;
using System.Collections.Generic;

namespace Fayora.Contracts.AdminModule.ChatbotMonitoring;

public record GetChatbotSessionsResponse(
    Guid Id,
    string DeviceId,
    Guid? UserId,
    string? UserName,
    int MessageCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastMessageAt
);

public record GetChatbotSessionMessagesResponse(
    long Id,
    string Role,
    string Content,
    DateTimeOffset CreatedAt
);

public record GetChatbotStatsResponse(
    int TotalSessions,
    int TotalMessages,
    int SessionsToday,
    int MessagesToday,
    double AverageMessagesPerSession
);
