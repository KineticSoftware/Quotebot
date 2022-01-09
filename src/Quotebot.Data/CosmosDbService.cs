using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Quotebot.Data.Entities;
using System.Net;
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
            try
            {
                var result = await _container.CreateItemAsync(message, new PartitionKey(message.Author.Id));
                return result.StatusCode switch
                {
                    HttpStatusCode.Created => true,
                    _ => false,
                };
                }
            catch (CosmosException ex)
            {
                if(ex.StatusCode != HttpStatusCode.Conflict)
                {
                    _logger.LogError(ex.ToString());
                }
                return false;
            }
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