using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using User = Quotebot.Domain.Entities.User;

namespace Quotebot.Data;

public class CosmosDbService(Container container, ILogger<CosmosDbService> logger) : IDataService
{
    private readonly ILogger _logger = logger;
    private readonly Container _container = container ?? throw new ArgumentNullException(nameof(container));

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

    public async Task<Quoted> FindQuoteById(string id)
    {
        QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", id);
        var query = _container.GetItemQueryIterator<Quoted>(queryDefinition, null, new QueryRequestOptions() { MaxItemCount = 1 });

        while (query.HasMoreResults)
        {
            foreach (var quote in await query.ReadNextAsync())
            {
                return quote;
            }
        }

        return new($"No Quote Found by Id: {id}");
    }

    public async IAsyncEnumerable<Quoted> FindQuotesByChannelAsync(string messageLike, string channelName, int take = 5)
    {
        QueryDefinition queryDefinition = new QueryDefinition(
                "SELECT * FROM c WHERE c.Channel.Name = @channelName AND CONTAINS(c.CleanContent, @messageLike, true) ORDER BY c.Timestamp DESC")
            .WithParameter("@channelName", channelName)
            .WithParameter("@messageLike", messageLike);
        var query = _container.GetItemQueryIterator<Quoted>(queryDefinition,
            null, new QueryRequestOptions() { MaxItemCount = take, MaxConcurrency = 5});

        while (query.HasMoreResults)
        {
            foreach (var record in await query.ReadNextAsync())
            {
                yield return record;
            }
        }
    }

    public async IAsyncEnumerable<Quoted> FindQuotesByGuildAsync(string messageLike, int take = 25)
    {
        QueryDefinition queryDefinition = new QueryDefinition(
                "SELECT * FROM c WHERE CONTAINS(c.CleanContent, @messageLike, true) ORDER BY c.Timestamp DESC")
            .WithParameter("@messageLike", messageLike);
        var query = _container.GetItemQueryIterator<Quoted>(queryDefinition,
            null, new QueryRequestOptions() { MaxItemCount = take, MaxConcurrency = 5 });

        while (query.HasMoreResults)
        {
            foreach (var record in await query.ReadNextAsync())
            {
                yield return record;
            }
        }
    }

    public async Task<Quoted> GetRandomQuoteInChannel(string channelName)
    {
        var count = await GetCountOfQuotesInChannel(channelName);

        if (count is 0)
            return new("No Quotes yet.");

        var randomNum = new Random().NextInt64(count);

        var queryDefinition = new QueryDefinition($"SELECT * FROM c WHERE c.Channel.Name = @channelName OFFSET @offset LIMIT 1")
            .WithParameter("@channelName", channelName)
            .WithParameter("@offset", randomNum);

        var query = _container.GetItemQueryIterator<Quoted>(queryDefinition,
            null, new QueryRequestOptions() { MaxItemCount = 1, MaxBufferedItemCount = 0, MaxConcurrency = 1 });

        while (query.HasMoreResults)
        {
            foreach (var quote in await query.ReadNextAsync())
            {
                return quote;
            }
        }

        return new("Unable to get random quote");
    }

    private async Task<int> GetCountOfQuotesInChannel(string channelName)
    {
        var countQueryDefinition = new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE c.Channel.Name = @channelName")
            .WithParameter("@channelName", channelName);
        var query = _container.GetItemQueryIterator<int>(countQueryDefinition,
            null, new QueryRequestOptions() {MaxItemCount = 1, MaxBufferedItemCount = 0, MaxConcurrency = 1});

        while (query.HasMoreResults)
        {
            foreach (var countOfRecords in await query.ReadNextAsync())
            {
                return countOfRecords;
            }
        }

        return default;
    }

    public async Task<Quoted> GetRandomQuoteInServer()
    {
        var count = await GetCountOfQuotesInServer();

        if (count is 0)
            return new("No Quotes yet.");

        var randomNum = new Random().NextInt64(count);

        var queryDefinition = new QueryDefinition($"SELECT * FROM c OFFSET @offset LIMIT 1")
            .WithParameter("@offset", randomNum);

        var query = _container.GetItemQueryIterator<Quoted>(queryDefinition,
            null, new QueryRequestOptions() { MaxItemCount = 1, MaxBufferedItemCount = 0, MaxConcurrency = 1 });

        while (query.HasMoreResults)
        {
            foreach (var quote in await query.ReadNextAsync())
            {
                return quote;
            }
        }

        return new("Unable to get random quote");
    }

    private async Task<int> GetCountOfQuotesInServer()
    {
        var countQueryDefinition = new QueryDefinition("SELECT VALUE COUNT(1) FROM c");
        var query = _container.GetItemQueryIterator<int>(countQueryDefinition,
            null, new QueryRequestOptions() { MaxItemCount = 1, MaxBufferedItemCount = 0, MaxConcurrency = 1 });

        while (query.HasMoreResults)
        {
            foreach (var countOfRecords in await query.ReadNextAsync())
            {
                return countOfRecords;
            }
        }

        return default;
    }
}