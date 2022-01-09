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

            var completeMessage = await Context.GetCompleteMessage(message);
            if (completeMessage.Author.IsBot)
            {
                await RespondAsync($"Sorry, you can't add quotes from bots.");
                return;
            }

            await completeMessage.AddReactionAsync(BotExtensions.QuoteBotEmote());

            var quote = new Quoted(completeMessage);
            quote.Author = await Context.GetGuildUserName(completeMessage.Author);

            var result = await _dataService.TryCreateQuoteRecord(quote);
            if(!result)
            {
                await RespondAsync($"This quote was already added.");
                return;
            }

            var response = new StringBuilder()
                .AppendLine($"> *{quote.Author?.Nickname ?? quote.Author?.Username} : {completeMessage.Content}*");

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
