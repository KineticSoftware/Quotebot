using Discord.Interactions;
using Quotebot.Domain.Validators;
using System.Text;

namespace Quotebot.Interactions.MessageCommand;

// ReSharper disable once UnusedType.Global
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

        await DeferAsync();

        var completeMessage = await Context.GetCompleteMessage(message);
        bool isValid = completeMessage.Validate(Context.User, async (validationException) =>
        {
            await FollowupAsync(validationException);
        });

        if (!isValid)
        {
            return;
        }

        Quoted quote = new(completeMessage)
        {
            Author = await Context.GetGuildUserName(completeMessage.Author)
        };

        bool recordCreated = await _dataService.TryCreateQuoteRecord(quote);
        if (!recordCreated)
        {
            await FollowupAsync($"{Context.User.Mention} this quote was added previously.");
            return;
        }

        await completeMessage.AddReactionAsync(BotEmotes.QuotedEmote());

        var response = new StringBuilder()
            .AppendLine($"> *{quote.Author.Nickname ?? quote.Author.Username} : {completeMessage.Content}*");

        await FollowupAsync($"{response}");
    }
}
