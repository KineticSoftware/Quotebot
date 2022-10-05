namespace Quotebot.Configuration
{
    internal class YoutubeConfiguration
    {
        public const string ConfigurationSectionName = "Youtube";

        public string ApiKey { get; init; } = string.Empty;
    }
}
