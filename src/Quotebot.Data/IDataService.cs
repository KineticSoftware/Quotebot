using Quotebot.Data.Entities;

namespace Quotebot.Data
{
    public interface IDataService
    {
        Task CreateQuoteRecord(Quoted message);

        Task<int> QuotesCountByUser(User user);

        Task<string> FindByQuote(string messageLike);
    }
}