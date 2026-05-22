namespace Fayora.Infrastructure.Settings;

public class GeminiSettings
{
    public const string SectionName = "GeminiSettings";
    public string ApiKey { get; init; } = null!;
    public string Model { get; init; } = "gemini-2.0-flash";
    public string BotUserId { get; init; } = "00000000-0000-0000-0000-000000000001";
}
