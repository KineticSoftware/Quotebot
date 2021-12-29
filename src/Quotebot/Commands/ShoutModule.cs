using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quotebot.Commands
{
    [Group("quote")]
    public class ShoutModule : ModuleBase<SocketCommandContext>
    {
        [Command("hello")]
        [Summary("Says hello to you")]
        public async Task SayHello()
        {
            //await Context.Channel.SendMessageAsync($"Hello {Context.User.Username}");
            await ReplyAsync($"Hello {Context.User.Username}");
        }

        [Command("time")]
        [Summary("Says the current time")]
        public async Task SayTime()
        {
            //await Context.Channel.SendMessageAsync($"Hello {Context.User.Username}");
            await ReplyAsync($"The current time is {DateTime.Now}");
        }
    }
}
