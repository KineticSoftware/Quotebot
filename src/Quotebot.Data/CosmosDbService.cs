﻿using Discord;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using User = Quotebot.Domain.Entities.User;

namespace Quotebot.Data;

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

        return new();
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
            .OrderByDescending(record => record.Timestamp)
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
        QueryDefinition queryDefinition =
            new QueryDefinition(
                    "SELECT * FROM c WHERE c.Channel.Name = @channelName AND CONTAINS(c.CleanContent, @messageLike, true ) ORDER BY c.Timestamp DESC")
                .WithParameter("@channelName", channelName)
                .WithParameter("@messageLike", messageLike);
        var query = _container.GetItemQueryIterator<Quoted>(queryDefinition, null, new QueryRequestOptions() { MaxItemCount = take});

        List<Quoted> results = new();
        while (query.HasMoreResults)
        {
            foreach (var quote in await query.ReadNextAsync())
            {
                results.Add(quote);
            }
        }

        return results;
    }

    public async IAsyncEnumerable<Quoted> FindQuotesByChannelAsync(string messageLike, string channelName, int take = 5)
    {
        QueryDefinition queryDefinition = new QueryDefinition(
                "SELECT * FROM c WHERE c.Channel.Name = @channelName AND CONTAINS(c.CleanContent, @messageLike, true ) ORDER BY c.Timestamp DESC")
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

    public async Task<Quoted> GetRandomQuoteInChannel(string channelName)
    {
        var count = await GetCountOfQuotesInChannel(channelName);

        if (count is 0)
            return new() {Content = "No Quotes yet."};

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

        return new() {Content = "Unable to get random quote"};
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
            return new() { Content = "No Quotes yet." };

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

        return new() { Content = "Unable to get random quote" };
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