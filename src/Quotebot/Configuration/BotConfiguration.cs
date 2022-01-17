namespace Quotebot.Configuration;

internal class BotConfiguration
{
    public const string ConfigurationSectionName = "Discord";

    public string Token { get; set; } = string.Empty;
    public ulong GuildId { get; set; }

}
