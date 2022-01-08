using Discord;

namespace Quotebot.Data
{
    public interface IDataService
    {

        Task CreateQuoteRecord(IMessage message);
    }
}