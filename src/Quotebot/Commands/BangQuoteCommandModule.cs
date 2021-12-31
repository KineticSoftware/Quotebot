using Discord.Commands;

namespace Quotebot.Commands
{
    [Group("quote")]
    public class BangQuoteCommandModule : ModuleBase<SocketCommandContext>
    {
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
    }
}
