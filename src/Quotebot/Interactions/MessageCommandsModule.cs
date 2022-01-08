using Discord.Interactions;
using Quotebot.Data;
using Quotebot.Data.Entities;
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

            await _dataService.CreateQuoteRecord(new Quoted(completeMessage));

            var response = new StringBuilder()
                .AppendLine($"*{completeMessage.Content}* by {completeMessage.Author.Username} quoted!");

            await RespondAsync($"{response}");
        }

        [UserCommand("Count of Quotes")]
        public async Task GetUserQuoteCount(IUser user)
        {
            var countedQuotes = await _dataService.QuotesCountByUser(new User(user));

            await RespondAsync($"{user.Username} has been quoted {countedQuotes} times");
        }
    }
}
