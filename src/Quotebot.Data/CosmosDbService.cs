using Discord;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using User = Quotebot.Domain.Entities.User;

namespace Quotebot.Data
{
    public class CosmosDbService : IDataService
    {
        private readonly ILogger _logger;
        private readonly Container _container;

        public CosmosDbService(Container container, ILogger<CosmosDbService> logger)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _logger = logger;
        }

        public async Task<bool> TryCreateQuoteRecord(Quoted message)
        {
            try
            {
                var result = await _container.CreateItemAsync(message, new PartitionKey(message.Author.Id)).ConfigureAwait(false);
                return result.StatusCode switch
                {
                    HttpStatusCode.Created => true,
                    _ => false,
                };
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode != HttpStatusCode.Conflict)
                {
                    _logger.LogError(ex.ToString());
                }

                return false;
            }
        }

        public async Task<int> QuotesCountByUser(User user)
        {
            return await _container.GetItemLinqQueryable<Quoted>()
                .Where(item => 
                    item.Author.Mention == user.Mention)
                .CountAsync()
                .ConfigureAwait(false);
        }

        // broken
        //public async Task<(bool Success, Quoted Quoted)> FindById(string messageId)
        //{
        //    using var setIterator = _container.GetItemLinqQueryable<Quoted>(allowSynchronousQueryExecution: true)
        //        .Where(record =>
        //            record.Id == messageId)
        //        .Take(5)
        //        .ToFeedIterator();

        //    List<Quoted> results = new();

        //    while (setIterator.HasMoreResults)
        //    {
        //        foreach (var quote in await setIterator.ReadNextAsync().ConfigureAwait(false))
        //        {
        //           return (true, quote);
        //        }
        //    }

        //    return (false, new Quoted());
        //}

        public async Task<string> FindByQuoteInServer(string messageLike, int take = 5)
        {
            var results = await FindQuotesInServer(messageLike, take);

            var enumerable = results as Quoted[] ?? results.ToArray();
            if (!enumerable.Any())
            {
                return $"No quotes found containing the text *{messageLike}* in this server.";
            }

            StringBuilder stringBuilder = new();

            foreach (var quote in enumerable)
            {
                stringBuilder
                    .AppendLine()
                    .AppendLine(
                        $"{quote.CreatedAt.ToString("d")} - **{quote.Author.Nickname ?? quote.Author.Username}** in #{quote.Channel.Name} : {quote.Content}");
            }

            var resultString = stringBuilder.ToString();

            return resultString;
        }

        public async Task<string> FindByQuote(string messageLike, string channelName, int take = 5)
        {
            IEnumerable<Quoted> results = await FindQuotesByChannel(messageLike, channelName, take);
            var quotesList = results.ToList();
            if (!quotesList.Any())
                return $"No quotes found containing the text *{messageLike}* in this channel.";


            StringBuilder stringBuilder = new();
            foreach (var quote in quotesList)
            {
                stringBuilder
                    .AppendLine()
                    .AppendLine(
                        $"{quote.CreatedAt.ToString("d")} - **{quote.Author.Nickname ?? quote.Author.Username}** : {quote.Content}");
            }

            var result = stringBuilder.ToString();
            if (string.IsNullOrWhiteSpace(result))
                return $"No quotes found containing the text *{messageLike}* in this channel.";

            return result;
        }

        public async Task<IEnumerable<Quoted>> FindQuotesInServer(string messageLike, int take = 5)
        {
            var iterator = await _container.GetItemQueryIterator<Quoted>()
                .ReadNextAsync()
                .ConfigureAwait(false);

            if (!iterator.Any())
            {
                return Enumerable.Empty<Quoted>();
            }

            using var setIterator = _container.GetItemLinqQueryable<Quoted>(allowSynchronousQueryExecution: true)
                .Where(record =>
                    record.CleanContent != null &&
                    record.CleanContent.Contains(messageLike, StringComparison.InvariantCultureIgnoreCase))
                .Take(take)
                .ToFeedIterator();

            List<Quoted> results = new();

            while (setIterator.HasMoreResults)
            {
                foreach (var quote in await setIterator.ReadNextAsync().ConfigureAwait(false))
                {
                    results.Add(quote);
                }
            }

            return results;
        }

        public async Task<IEnumerable<Quoted>> FindQuotesByChannel(string messageLike, string channelName, int take = 5)
        {
            var iterator = await _container.GetItemQueryIterator<Quoted>()
                .ReadNextAsync()
                .ConfigureAwait(false);

            if (!iterator.Any())
                return Enumerable.Empty<Quoted>();

            using var setIterator = _container.GetItemLinqQueryable<Quoted>(true)
                .Where(record => 
                    record.Channel.Name == channelName && 
                    record.CleanContent != null &&
                    record.CleanContent.Contains(messageLike, StringComparison.InvariantCultureIgnoreCase))
                .Take(take)
                .ToFeedIterator();

            List<Quoted> results = new();

            while (setIterator.HasMoreResults)
            {
                foreach (var quote in await setIterator.ReadNextAsync().ConfigureAwait(false))
                {
                    results.Add(quote);
                }
            }

            return results;
        }

        public async Task<IEnumerable<Quoted>> FindQuotesByUserInChannel(IUser user, string channelName, string messageLike)
        {
            var iterator = await _container.GetItemQueryIterator<Quoted>()
                .ReadNextAsync()
                .ConfigureAwait(false);

            if (!iterator.Any())
            {
                return Enumerable.Empty<Quoted>();
            }

            //record.Channel.Name == channelName &&
            using var setIterator = _container.GetItemLinqQueryable<Quoted>(allowSynchronousQueryExecution: true)
                .Where(record =>
                    record.Author.Mention == user.Mention &&
                    
                    record.CleanContent != null &&
                    record.CleanContent.Contains(messageLike, StringComparison.InvariantCultureIgnoreCase))
                .Take(5)
                .ToFeedIterator();

            List<Quoted> results = new();

            while (setIterator.HasMoreResults)
            {
                foreach (var quote in await setIterator.ReadNextAsync().ConfigureAwait(false))
                {
                    results.Add(quote);
                }
            }

            return results;
        }
    }
}