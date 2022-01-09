namespace Quotebot.Data
{
    internal class DataConstants
    {
        public const string DatabaseId = "QuoteMage";
        public const string ContainerId = "Quotes";
        public const string PrimaryPartitionKey = "/Author/Id";
    }
}