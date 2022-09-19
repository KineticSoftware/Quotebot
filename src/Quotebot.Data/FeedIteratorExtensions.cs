using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Quotebot.Data;

public static class FeedIteratorExtensions
{
    public static async IAsyncEnumerable<TModel> ToAsyncEnumerable<TModel>(this FeedIterator<TModel> iterator)
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