using Discord.Commands;
using System.Reflection;
using System.Text;

// ReSharper disable StringLiteralTypo

namespace Quotebot.Commands;

[Group("quote")]
public class QuoteCommandModule : ModuleBase<SocketCommandContext>
{
    IDataService _dataService;

    public QuoteCommandModule(IDataService dataService)
    {
        _dataService = dataService;
    }

    [Command("hello")]
    [Summary("Says hello to you")]
    public async Task SayHello()
    {
        await ReplyAsync($"Hello {Context.User.Username}");
    }

    [Command("hey")]
    [Summary("Idiocracy Quote")]
    public async Task SayGoAway()
    {
        await ReplyAsync($"{Context.User.Username} fuck you, I'm eating!");
    }

    [Command("version")]
    [Summary("Gets the bot's current version")]
    public async Task SayVersion()
    {
        await ReplyAsync($"My current version is {Assembly.GetEntryAssembly()?.GetName().Version}");
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

            if (string.IsNullOrWhiteSpace(completeMessage.CleanContent))
            {
                await ReplyAsync($"No actual text was found. You can only quote text chat.");
                return;
            }

            if (completeMessage.Embeds.Count > 0 || completeMessage.Attachments.Count > 0)
            {
                await ReplyAsync($"An embed or an attachment was found. You can currently only quote text chat.");
                return;
            }

            var quote = new Quoted(completeMessage);
            quote.Author = await Context.GetGuildUserName(completeMessage.Author);

            var result = await _dataService.TryCreateQuoteRecord(quote);
            if (!result)
            {
                await ReplyAsync($"This quote was already added.");
                return;
            }

            await completeMessage.AddReactionAsync(BotEmotes.QuotedEmote());

            var response = new StringBuilder()
                .AppendLine($"> *{quote.Author.Nickname ?? quote.Author.Username} : {completeMessage.Content}*");

            await ReplyAsync($"{response}");
        }
    }
}