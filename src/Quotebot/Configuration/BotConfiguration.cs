namespace Quotebot.Configuration;

internal class DiscordConfiguration
{
    public const string ConfigurationSectionName = "Discord";

    public string Token { get; init; } = string.Empty;
    public ulong GuildId { get; init; }
    public ulong GeneralChannelId { get; init; }
}
