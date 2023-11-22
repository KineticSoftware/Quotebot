using Discord.Interactions;
using Quotebot.Interactions.AutoComplete;

// ReSharper disable StringLiteralTypo

namespace Quotebot.Interactions.SlashCommand;

// ReSharper disable once UnusedType.Global
public class FindQuoteSlashCommandsModule : InteractionModuleBase<SocketInteractionContext>
{
    readonly IDataService _dataService;
    public FindQuoteSlashCommandsModule(IDataService dataService)
    {
        _dataService = dataService;
    }

    [SlashCommand("findserverquote", "Finds a quote across all channels in this server")]
    public async Task FindServerQuote([Autocomplete(typeof(SearchQuotesByGuildAutoCompleteHandler)), Summary("query", "Partial match text of the quote to find.")]
        string query)
    {
        // it's not super obvious but SearchQuotesByGuildAutoCompleteHandler will return an id of the picked quote. 
        if (!long.TryParse(query, out _))
        {
            await RespondAsync(
                $"That quote doesn't seem to exist {Context.User.Mention}. You need to pick from the suggestions of saved quotes.");
            return;
        }

        await DeferAsync();
        var quote = await _dataService.FindQuoteById(query);
        await FollowupAsync(
            $"{quote.CreatedAt:d} - **{quote.Author.Nickname ?? quote.Author.Username}** in #{quote.Channel?.Name}: {quote.Content}");
    }

    [SlashCommand("findquote", "Search for a specific quote by user")]
    public async Task FindQuote(
        [Autocomplete(typeof(SearchQuotesByChannelAutoCompleteHandler)), Summary("query", "Partial match text of the quote to find.")]
        string query
    )
    {
        // it's not super obvious but SearchQuotesByChannelAutoCompleteHandler will return an id of the picked quote. 
        if (!long.TryParse(query, out _))
        {
            await RespondAsync(
                $"That quote doesn't seem to exist {Context.User.Mention}. You need to pick from the suggestions of saved quotes.");
            return;
        }

        await DeferAsync();
        var quote = await _dataService.FindQuoteById(query);
        await FollowupAsync(
            $"{quote.CreatedAt:d} - **{quote.Author.Nickname ?? quote.Author.Username}** : {quote.Content}");
    }

    [SlashCommand("randomquote", "Get a random quote")]
    public async Task FindRandomQuote(QuoteChoice choice)
    {
        await DeferAsync();

        var quote = choice switch
        {
            QuoteChoice.Channel => await _dataService.GetRandomQuoteInChannel(Context.Channel.Name),
            QuoteChoice.Server => await _dataService.GetRandomQuoteInServer(),
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, null)
        };

        await FollowupAsync(
            $"{quote.CreatedAt:d} - **{quote.Author.Nickname ?? quote.Author.Username}** : {quote.Content}");
    }


    public enum QuoteChoice
    {
        Channel,
        Server
    }
}