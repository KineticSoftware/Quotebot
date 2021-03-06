namespace Quotebot.Data;

public interface IDataService
{
    Task<bool> TryCreateQuoteRecord(Quoted message);

    Task<int> QuotesCountByUser(User user);

    Task<string> FindByQuote(string messageLike, string channelName, int take = 5);

    Task<string> FindByQuoteInServer(string messageLike, int take = 5);
}
