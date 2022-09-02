using Discord;

namespace Quotebot.Data;

public interface IDataService
{
    //Task<(bool Success, Quoted Quoted)> FindById(string messageId);

    Task<bool> TryCreateQuoteRecord(Quoted message);

    Task<int> QuotesCountByUser(User user);

    Task<string> FindByQuote(string messageLike, string channelName, int take = 5);

    Task<string> FindByQuoteInServer(string messageLike, int take = 5);

    Task<Quoted> FindQuoteById(string id);

    Task<IEnumerable<Quoted>> FindQuotesInServer(string messageLike, int take = 5);

    Task<IEnumerable<Quoted>> FindQuotesByChannel(string messageLike, string channelName, int take = 5);

    //IAsyncEnumerable<Quoted> FindQuotesByChannelAsync(string messageLike, string channelName, int take = 5);

    Task<IEnumerable<Quoted>> FindQuotesByUserInChannel(IUser user, string channelName, string messageLike, int take = 5);
}
