namespace Quotebot.Data.Configuration;

internal class CosmosConfiguration
{
    internal const string ConfigurationSectionName = "CosmosDb";

    public string Url { get; set; } = string.Empty;
    public string Authorization { get; set; } = string.Empty;
    public bool AlwaysRebuildContainer { get; set; }
}
