using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Quotebot.Data.Entities;
using System.Text;

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

        public async Task<bool> TryCreateQuoteRecord(Quoted message)
        {
            using var iterator = _container.GetItemLinqQueryable<Quoted>()
               .Where(record => record.DiscordMessageId == message.DiscordMessageId).ToFeedIterator();

            if (iterator.HasMoreResults)
            {
                foreach(var item in await iterator.ReadNextAsync())
                {
                    return false;
                }
                 
            }

            await _container.CreateItemAsync(message, new PartitionKey(Convert.ToString(message.Id)));
            return true;
        }

        public async Task<int> QuotesCountByUser(Entities.User user)
        {
            return await _container.GetItemLinqQueryable<Quoted>()
                .Where(item => item.Author != null && item.Author.Id == user.Id)
                .CountAsync();
        }

        public async Task<string> FindByQuote(string messageLike, int take = 5)
        {
            var iterator = await _container.GetItemQueryIterator<Quoted>().ReadNextAsync();
            if(!iterator.Any())
            {
                return $"No quotes found containg the text *{messageLike}*";
            }

            using var setIterator = _container.GetItemLinqQueryable<Quoted>(allowSynchronousQueryExecution: true)
                                 .Where(record => record != null && record.CleanContent != null && record.CleanContent.Contains(messageLike, StringComparison.InvariantCultureIgnoreCase))
                                 .Take(take)
                                 .ToFeedIterator();
            
            List<Quoted> results = new();

            StringBuilder stringBuilder = new();
            while (setIterator.HasMoreResults)
            {
                foreach (var quote in await setIterator.ReadNextAsync())
                {
                    stringBuilder
                        .AppendLine()
                        .AppendLine($"{quote.CreatedAt.ToString("d")} - **{quote.Author?.Nickname ?? quote.Author?.Username}** : {quote.Content}");
                }
            }

            return stringBuilder.ToString();
        }
    }
}