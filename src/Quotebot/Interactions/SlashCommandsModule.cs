using Discord.Interactions;
using Quotebot.Interactions.AutoComplete;

// ReSharper disable StringLiteralTypo

namespace Quotebot.Interactions;

// ReSharper disable once UnusedType.Global
public class SlashCommandsModule : InteractionModuleBase<SocketInteractionContext>
{
    readonly IDataService _dataService;
    public SlashCommandsModule(IDataService dataService)
    {
        _dataService = dataService;
    }

    [SlashCommand("findquote-legacy", "Finds a quote")]
    public async Task FindQuoteLegacy(string text, int limit = 5)
    {
        await DeferAsync();

        var results = await _dataService.FindByQuote(text, Context.Channel.Name, limit);

        await FollowupAsync(results);
    }

    [SlashCommand("findserverquote", "Finds a quote across all channels in this server")]
    public async Task FindServerQuote(string text, int limit = 5)
    {
        await DeferAsync();

        var results = await _dataService.FindByQuoteInServer(text, limit);

        await FollowupAsync(results);
    }

    [SlashCommand("findquote", "Search for a specific quote by user")]
    public async Task FindQuote(
        [Autocomplete(typeof(SearchQuotesAutoCompleteHandler)), Summary("query", "The query for the item to search")]
        string query
    )
    {
        // it's not super obvious but SearchQuotesAutoCompleteHandler should return an id of the picked quote. 
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
}