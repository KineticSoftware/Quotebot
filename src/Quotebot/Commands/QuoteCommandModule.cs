using Discord.Commands;
using Quotebot.Data;
using System.Reflection;
using System.Text;

namespace Quotebot.Commands
{
    [Group("quote")]
    public class QuoteCommandModule : ModuleBase<SocketCommandContext>
    {
        IDataService _dataService;

        public QuoteCommandModule(IDataService dataService)
        {
            _dataService = dataService;
        }
        
        [Command("hello")]
        [Summary("Says hello to you")]
        public async Task SayHello()
        {
            await ReplyAsync($"Hello {Context.User.Username}");
        }

        [Command("hey")]
        [Summary("Says the current time")]
        public async Task SayGoAway()
        {
            await ReplyAsync($"{Context.User.Username} fuck you, I'm eating!");
        }

        [Command("version")]
        [Summary("Gets the bot's current version")]
        public async Task SayVersion()
        {
            await ReplyAsync($"My current version is {Assembly.GetEntryAssembly()?.GetName().Version}");
        }

        [Command("find")]
        [Summary("Find a Quote")]
        public async Task SayTime([Remainder] string text)
        {

            StringBuilder stringBuilder = new();
            await foreach(var quote in _dataService.FindByQuote(text))
            {
                stringBuilder.AppendLine($"{quote.Content} by {quote.Author?.Username} on {quote.CreatedAt.ToString("d")}");
            }

            await ReplyAsync(stringBuilder.ToString());
        }
    }
}
