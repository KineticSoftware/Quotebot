using Discord.Interactions;
using Quotebot.Data;
using System.Text;

namespace Quotebot.Interactions
{
    public class MessageCommandsModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IDataService _dataService;

        public MessageCommandsModule(IDataService dataService)
        {
            _dataService = dataService;
        }
        [MessageCommand("Save Quote!")]
        public async Task AddQuote(IMessage rawMessage)
        {
            if (rawMessage is not SocketUserMessage message)
                return;
            await message.AddReactionAsync(BotExtensions.QuoteBotEmote());
            var completeMessage = await Context.GetCompleteMessage(message);

            await _dataService.CreateQuoteRecord(completeMessage);

            var response = new StringBuilder()
                .Append($"*{completeMessage.Author.Username}* quoted!")
                .AppendLine($"*{completeMessage.Content}*");
            await RespondAsync($"{response}");
        }
    }
}
