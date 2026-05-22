using System;
using Fayora.Domain.Common.Entity;

namespace Fayora.Domain.Entities.NotificationModule;

public class DeviceToken : AuditableEntity<Guid>
{
    public Guid? UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public string DeviceType { get; private set; } = null!; // "Android", "iOS", "Web"
    public DateTimeOffset LastActiveAt { get; private set; }

    private DeviceToken() { }

    public static DeviceToken Create(Guid? userId, string token, string deviceType)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));

        return new DeviceToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            DeviceType = string.IsNullOrWhiteSpace(deviceType) ? "Unknown" : deviceType,
            LastActiveAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateActivity(Guid? userId, string deviceType)
    {
        UserId = userId;
        DeviceType = string.IsNullOrWhiteSpace(deviceType) ? DeviceType : deviceType;
        LastActiveAt = DateTimeOffset.UtcNow;
        Updated();
    }
}
