using System;
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

        public async Task<int> QuotesCountByUser(User user)
        {
            return await _container.GetItemLinqQueryable<Quoted>()
                .Where(item => item.Author.Mention == user.Mention)
                .CountAsync();
        }

        public async Task<string> FindByQuoteInServer(string messageLike, int take = 5)
        {
            var iterator = await _container.GetItemQueryIterator<Quoted>().ReadNextAsync();
            if (!iterator.Any())
                return BuildNoResultResponse(messageLike);

            using var setIterator = _container.GetItemLinqQueryable<Quoted>(allowSynchronousQueryExecution: true)
                                 .Where(record => record.CleanContent != null && record.CleanContent.Contains(messageLike, StringComparison.InvariantCultureIgnoreCase))
                                 .Take(take)
                                 .ToFeedIterator();

            return await BuildResponseAsync(setIterator,
                (quote) => $"{BuildResponsePrefix(quote)} in #{quote.Channel.Name} : {quote.Content}",
                messageLike);
        }

        public async Task<string> FindByQuote(string messageLike, string channelName, int take = 5)
        {
            var iterator = await _container.GetItemQueryIterator<Quoted>().ReadNextAsync();
            if(iterator.All(item => item.Channel.Name != channelName))
                return BuildNoResultResponse(messageLike);

            using var setIterator = _container.GetItemLinqQueryable<Quoted>(true)
                                 .Where(record => record.Channel.Name == channelName && record.CleanContent != null && record.CleanContent.Contains(messageLike, StringComparison.InvariantCultureIgnoreCase))
                                 .Take(take)
                                 .ToFeedIterator();

            return await BuildResponseAsync(setIterator,
                (quote) => $"{BuildResponsePrefix(quote)} : {quote.Content}",
                messageLike);
        }

        public string GetRandomQuotesByUser(User user, int take = 1)
        {
            var quotes = _container.GetItemLinqQueryable<Quoted>()
                .Where(item => item.Author.Mention == user.Mention)
                .ToArray();
            return AggregateRandomQuotes(quotes, take);
        }

        public string GetRandomQuotesByChannel(string channelName, int take = 1)
        {
            var quotes = _container.GetItemLinqQueryable<Quoted>()
                .Where(item => item.Channel.Name == channelName)
                .ToArray();
            return AggregateRandomQuotes(quotes, take);
        }

        private string AggregateRandomQuotes(Quoted[] quotes, int take)
        {
            Random rand = new();
            var results = new Quoted[take];
            for (var i = 0; i < take; i++)
                results[i] = quotes[rand.Next(quotes.Length - 1)];
            return BuildResponse(results, (quote) => $"{BuildResponsePrefix(quote)} : {quote.Content}", string.Empty);
        }

        private static async Task<string> BuildResponseAsync(FeedIterator<Quoted> setIterator, Func<Quoted, string> successEntryBuilder, string messageLike)
        {
            StringBuilder stringBuilder = new();
            while (setIterator.HasMoreResults)
                foreach (var quote in await setIterator.ReadNextAsync())
                    stringBuilder
                        .AppendLine()
                        .AppendLine(successEntryBuilder(quote));

            var result = $"{stringBuilder}";
            if (string.IsNullOrWhiteSpace(result))
                return BuildNoResultResponse(messageLike);

            return result;
        }

        private static string BuildResponse(IEnumerable<Quoted> resultSet, Func<Quoted, string> successEntryBuilder, string messageLike)
        {
            StringBuilder stringBuilder = new();
            foreach (var quote in resultSet)
                stringBuilder
                    .AppendLine()
                    .AppendLine(successEntryBuilder(quote));

            var result = $"{stringBuilder}";
            if (string.IsNullOrWhiteSpace(result))
                return BuildNoResultResponse(messageLike);

            return result;
        }

        private static string BuildResponsePrefix(Quoted? quote) => 
            $"{quote?.CreatedAt.ToString("d") ?? string.Empty} - **{quote?.Author?.Nickname ?? quote?.Author?.Username ?? string.Empty}**";

        private static string BuildNoResultResponse(string messageLike) => 
            $"No quotes found containing the text *{messageLike}* in this channel.";
    }
}
