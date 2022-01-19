using Discord.Interactions;
using System.Text;

namespace Quotebot.Interactions;

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
        if (completeMessage.Author.IsBot)
        {
            await FollowupAsync($"Sorry, you can't add quotes from bots.");
            return;
        }

        if (string.IsNullOrWhiteSpace(completeMessage.CleanContent))
        {
            await FollowupAsync($"No actual text was found. You can only quote text chat.");
            return;
        }

        if (message.Embeds.Count > 0 || message.Attachments.Count > 0)
        {
            await FollowupAsync($"An embed or an attachment was found. You can currently only quote text chat.");
            return;
        }

        var quote = new Quoted(completeMessage);
        quote.Author = await Context.GetGuildUserName(completeMessage.Author);

        var result = await _dataService.TryCreateQuoteRecord(quote);
        if (!result)
        {
            await FollowupAsync($"This quote was already added.");
            return;
        }

        await completeMessage.AddReactionAsync(BotExtensions.QuoteBotEmote());

        var response = new StringBuilder()
            .AppendLine($"> *{quote.Author?.Nickname ?? quote.Author?.Username} : {completeMessage.Content}*");

        await FollowupAsync($"{response}");
    }

    [UserCommand("Count of Quotes")]
    public async Task GetUserQuoteCount(IUser user)
    {
        await DeferAsync();
        var guildUser = await Context.GetGuildUserName(user);
        var countedQuotes = await _dataService.QuotesCountByUser(guildUser);

        var respsonse = countedQuotes switch
        {
            0 => $"{guildUser.Nickname ?? guildUser.Username} has never been quoted.",
            1 => $"{guildUser.Nickname ?? guildUser.Username} has been quoted once.",
            _ => $"{guildUser.Nickname ?? guildUser.Username} has been quoted {countedQuotes} times."
        };

        await FollowupAsync(respsonse);
    }
}
