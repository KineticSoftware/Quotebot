namespace Quotebot.Data;

internal static class DataConstants
{
    public const string DatabaseId = "QuoteMage";
    public const string ContainerId = "Quotes";
    public const string PrimaryPartitionKey = "/Author/Id";
}
