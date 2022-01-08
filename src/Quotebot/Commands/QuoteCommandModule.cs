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

        [Command("time")]
        [Summary("Says the current time")]
        public async Task SayTime()
        {
            await ReplyAsync($"The current time is {DateTime.Now}");
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
    }
}
