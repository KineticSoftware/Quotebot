using Discord;

namespace Quotebot.Data
{
    public interface IDataService
    {
        Task Initialize();

        Task CreateQuoteRecord(IMessage message);
    }
}