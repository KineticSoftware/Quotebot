using Discord.Commands;
using Quotebot.Data;
using Quotebot.Data.Entities;
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
        [Summary("Idiocracy Quote")]
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
            var results = await _dataService.FindByQuote(text);
            if(results == Enumerable.Empty<Quoted>())
            {
                await ReplyAsync($"No quotes found containg the text {text}");
                return;
            }

            foreach(var quote in results)
            {
                stringBuilder.AppendLine($"{quote.Content}")
                    .AppendLine($"*by {quote.Author?.Username} on {quote.CreatedAt.ToString("d")}*");
            }

            await ReplyAsync(stringBuilder.ToString());
        }
    }
}
