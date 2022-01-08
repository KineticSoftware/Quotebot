using Discord;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Quotebot.Data.Entities;

namespace Quotebot.Data
{
    public class CosmosDbService : IDataService
    {
        private readonly ILogger _logger;
        private readonly Container _container;

        public CosmosDbService(Container container, ILogger<CosmosDbService> logger)
        {
            if(container is null)
                throw new ArgumentNullException(nameof(container));

            _container = container;
            _logger = logger;
        }

        public async Task CreateQuoteRecord(Quoted message)
        {
            await _container.CreateItemAsync(message, new PartitionKey(Convert.ToString(message.Id)));
        }

        public async Task<int> QuotesCountByUser(Entities.User user)
        {
            return await _container.GetItemLinqQueryable<Quoted>()
                .Where(item => item.Author != null && item.Author.Id == user.Id)
                .CountAsync();
        }

        public async IAsyncEnumerable<Quoted> FindByQuote(string messageLike)
        {
            using var setIterator = _container.GetItemLinqQueryable<Quoted>()
                                 .Where(record => record.Content.Contains(messageLike))
                                 .ToFeedIterator();
            
            while (setIterator.HasMoreResults)
            {
                foreach (var item in await setIterator.ReadNextAsync())
                {
                    yield return item;
                }
            }
        }
    }
}