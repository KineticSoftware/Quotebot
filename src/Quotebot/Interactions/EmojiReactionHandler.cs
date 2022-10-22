using System.Text;
using Quotebot.Domain.Validators;

namespace Quotebot.Interactions
{
    public class EmojiReactionHandler
    {
        private readonly IDataService _dataService;

        public EmojiReactionHandler(IDataService dataService)
        {
            _dataService = dataService;
        }

        internal async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> cachedUserMessage, Cacheable<IMessageChannel, ulong> cachedChannelMessage, SocketReaction reaction)
        {
            if (!reaction.Emote.Equals(BotEmotes.QuotedEmote()))
                return;

            var userMessage = await cachedUserMessage.GetOrDownloadAsync();
            if (userMessage is { Author.IsBot: true })
                return;

            var emotedValue = userMessage.Reactions.TryGetValue(BotEmotes.QuotedEmote(), out var emoteMetadata);
            if (!emotedValue || emoteMetadata.ReactionCount > 1 || reaction.User.Value.IsBot)
                return;

            var channelMessage = await cachedChannelMessage.GetOrDownloadAsync();
            if (channelMessage is not IGuildChannel)
                return;

            if (reaction.User.GetValueOrDefault() is not SocketGuildUser socketGuildUser)
                return;

            bool isValid = userMessage.Validate(socketGuildUser, async validationException =>
            {
                await userMessage.ReplyAsync(validationException);
            });

            if (!isValid)
                return;

            var quote = new Quoted(userMessage);

            var result = await _dataService.TryCreateQuoteRecord(quote);
            if (!result)
            {
                await userMessage.ReplyAsync($"{socketGuildUser.Mention} this quote was added previously.");
                return;
            }

            var response = new StringBuilder()
                .AppendLine($"_{socketGuildUser.Nickname} quoted via_ {BotEmotes.QuotedRaw}")
                .AppendLine($"> *{quote.Author.Nickname ?? quote.Author.Username} : {userMessage.Content}*");

            await userMessage.ReplyAsync($"{response}");
        }
    }
}
