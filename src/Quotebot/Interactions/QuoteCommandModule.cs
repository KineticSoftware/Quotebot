using Discord.Commands;
using System.Text;
using Quotebot.Domain.Validators;
using Quotebot.Commands;

namespace Quotebot.Interactions;

[Group("mage")]
public class QuoteCommandModule : ModuleBase<SocketCommandContext>
{
    readonly IDataService _dataService;

    public QuoteCommandModule(IDataService dataService)
    {
        _dataService = dataService;
    }

    [Command("find")]
    [Summary("Find a Quote")]
    public async Task FindQuotes(string text, int limit = 5)
    {
        var results = await _dataService.FindByQuote(text, Context.Channel.Name, limit);

        await ReplyAsync(results);
    }

    [Command("findserver")]
    [Summary("Finds a Quote in a Server")]
    public async Task FindServerQuotes(string text, int limit = 5)
    {
        var results = await _dataService.FindByQuoteInServer(text, limit);

        await ReplyAsync(results);
    }

    [Command("add")]
    [Summary("Records a quote!")]
    public async Task AddQuote()
    {
        if (Context.Guild is null)
        {
            await ReplyAsync($"Try using this command in a channel {Context.User.Username}");
            return;
        }

        if (Context.Message.ReferencedMessage is null)
        {
            await ReplyAsync($"Try using this command in a reply {Context.User.Username}");
        }
        else
        {
            var completeMessage = await Context.GetCompleteMessage(Context.Message.ReferencedMessage);


            var validator = completeMessage.Validate();
            if (!validator.IsValid)
            {
                await ReplyAsync(validator.validationException);
                return;
            }

            Quoted quote = new(completeMessage)
            {
                Author = await Context.GetGuildUserName(completeMessage.Author)
            };

            var result = await _dataService.TryCreateQuoteRecord(quote);
            if (!result)
            {
                await ReplyAsync($"{Context.User.Mention} this quote was added previously.");
                return;
            }

            await completeMessage.AddReactionAsync(BotEmotes.QuotedEmote());

            var response = new StringBuilder()
                .AppendLine($"> *{quote.Author.Nickname ?? quote.Author.Username} : {completeMessage.Content}*");

            await ReplyAsync($"{response}");
        }
    }
}