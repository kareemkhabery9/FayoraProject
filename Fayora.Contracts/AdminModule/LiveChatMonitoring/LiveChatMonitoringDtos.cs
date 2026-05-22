using System;

namespace Fayora.Contracts.AdminModule.LiveChatMonitoring;

public record GetAdminChatsResponse(
    Guid Id,
    Guid FirstUserId,
    string FirstUserName,
    Guid SecondUserId,
    string SecondUserName,
    string ScopeType,
    int MessageCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastMessageAt
);

public record GetAdminChatMessagesResponse(
    long Id,
    Guid SenderId,
    string SenderName,
    string Content,
    string Type,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt
);

public record GetChatStatsResponse(
    int TotalChats,
    int ChatsToday,
    int TotalMessages,
    int MessagesToday,
    int UnreadMessages
);
