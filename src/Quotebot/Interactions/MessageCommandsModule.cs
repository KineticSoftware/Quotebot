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

            if(message.Author.IsBot)
            {
                await RespondAsync($"Sorry, you can't add quotes from bots.");
                return;
            }

            await message.AddReactionAsync(BotExtensions.QuoteBotEmote());
            var completeMessage = await Context.GetCompleteMessage(message);

            await _dataService.CreateQuoteRecord(new Quoted(completeMessage));

            var response = new StringBuilder()
                .AppendLine($"{completeMessage.Content}")
                .AppendLine($"*by {completeMessage.Author.Username} quoted!*");

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
