namespace Quotebot.Data;

public interface IDataService
{
    //Task<(bool Success, Quoted Quoted)> FindById(string messageId);

    Task<bool> TryCreateQuoteRecord(Quoted message);

    Task<int> QuotesCountByUser(User user);

    Task<Quoted> FindQuoteById(string id);

    IAsyncEnumerable<Quoted> FindQuotesByChannelAsync(string messageLike, string channelName, int take = 25);

    IAsyncEnumerable<Quoted> FindQuotesByGuildAsync(string messageLike, int take = 25);

    Task<Quoted> GetRandomQuoteInChannel(string channelName);

    Task<Quoted> GetRandomQuoteInServer();
}
