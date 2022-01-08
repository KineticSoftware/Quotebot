using Quotebot.Data.Entities;

namespace Quotebot.Data
{
    public interface IDataService
    {
        Task CreateQuoteRecord(Quoted message);

        Task<int> QuotesCountByUser(User user);

        IAsyncEnumerable<Quoted> FindByQuote(string messageLike);
    }
}