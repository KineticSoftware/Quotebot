using Microsoft.Azure.Cosmos;

namespace Quotebot.Data;

internal static class FeedIteratorExtensions
{
    internal static async IAsyncEnumerable<TModel> ToAsyncEnumerable<TModel>(this FeedIterator<TModel> iterator)
    {
        while (iterator.HasMoreResults)
        {
            foreach (TModel record in await iterator.ReadNextAsync())
            {
                yield return record;
            }
        }
    }
}