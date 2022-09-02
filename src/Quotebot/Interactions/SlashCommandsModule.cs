using Discord.Interactions;
using Quotebot.Interactions.AutoComplete;
using System.Text.Json;
// ReSharper disable StringLiteralTypo

namespace Quotebot.Interactions;

// ReSharper disable once UnusedType.Global
public class SlashCommandsModule : InteractionModuleBase<SocketInteractionContext>
{
    IDataService _dataService;
    public SlashCommandsModule(IDataService dataService)
    {
        _dataService = dataService;
    }

    [SlashCommand("findquote-legacy", "Finds a quote")]
    public async Task FindQuote(string text, int limit = 5)
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
    public async Task FindQuoteBeta(
        
        [Summary("query", "The query for the item to search"),
         Autocomplete(typeof(SearchQuotesAutoCompleteHandler))]
        string id)
    {
        if (!long.TryParse(id, out _))
        {
            await RespondAsync($"Chill out {Context.User.Mention}, you're going too fast for me. You need to pick from a list of quotes you want to find and I'm not done searching yet.");
        }
        
        await DeferAsync();
        var quote = await _dataService.FindQuoteById(id);
        await FollowupAsync($"{quote.CreatedAt:d} - **{quote.Author.Nickname ?? quote.Author.Username}** : {quote.Content}");
    }
}