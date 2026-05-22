namespace Fayora.Infrastructure.Settings;

public class OpenAISettings
{
    public const string SectionName = "OpenAISettings";
    public string ApiKey { get; init; } = null!;
    public string Model { get; init; } = "gpt-4o-mini";
}
