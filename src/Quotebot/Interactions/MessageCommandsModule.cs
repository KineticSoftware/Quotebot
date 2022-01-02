using Discord.Interactions;

namespace Quotebot.Interactions
{
    public class MessageCommandsModule : InteractionModuleBase<SocketInteractionContext>
    {
        [MessageCommand("Save Quote!")]
        public async Task AddQuote(IMessage rawMessage)
        {
            if (rawMessage is not SocketUserMessage message)
                return;

            
            var emoji = Emote.Parse("<:quoted:926362503531872317>");
            await message.AddReactionAsync(emoji);
            
            await RespondAsync($"sneks {message.Author.Username}");
        }
    }
}
