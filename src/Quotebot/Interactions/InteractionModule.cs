using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Quotebot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quotebot.Interactions
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Slash Commands are declared using the [SlashCommand], you need to provide a name and a description, both following the Discord guidelines
        [SlashCommand("ping", "Recieve a pong")]
        // By setting the DefaultPermission to false, you can disable the command by default. No one can use the command until you give them permission
        [DefaultPermission(true)]
        public async Task Ping()
        {
            await RespondAsync("pong");
        }

        // [Summary] lets you customize the name and the description of a parameter
        [SlashCommand("echo", "Repeat the input")]
        public async Task Echo(string echo, [Summary(description: "mention the user")] bool mention = false)
        {
            await RespondAsync(echo + (mention ? Context.User.Mention : string.Empty));
        }


        [MessageCommand("Save Quote!")]
        public async Task AddQuote(IMessage rawMessage)
        {
            if (rawMessage is not SocketUserMessage message)
                return;

            await message.ReplyAsync("Quote!");
            await message.AddReactionAsync(new Emoji("☁"));
        }
    }
}
