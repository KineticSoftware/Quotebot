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
        public async Task FindQuotes(string text, int limit = 5)
        {
            var results = await _dataService.FindByQuote(text, Context.Channel.Id, limit);

            await ReplyAsync(results);
        }

        [Command("findserver")]
        [Summary("Finds a Quote in a Server")]
        public async Task FindServerQuotes(string text, int limit = 5)
        {
            var results = await _dataService.FindByQuoteInServer(text, limit);

            await ReplyAsync(results);
        }
    }
}
