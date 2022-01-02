using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quotebot.Interactions
{
    public class MessageCommandsModule : InteractionModuleBase<SocketInteractionContext>
    {
        [MessageCommand("Save Quote!")]
        public async Task AddQuote(IMessage rawMessage)
        {
            if (rawMessage is not SocketUserMessage message)
                return;
            await message.AddReactionAsync(BotExtensions.QuoteBotEmote());
            var completeMessage = await Context.GetCompleteMessage(message);
            var response = new StringBuilder()
                .AppendLine("...sneks")
                .AppendLine($"*{completeMessage.Author.Username}*")
                .AppendLine($"*{completeMessage.Content}*");
            await RespondAsync($"{response}");
        }
    }
}
