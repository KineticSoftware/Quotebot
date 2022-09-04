using Discord.Interactions;

namespace Quotebot.Interactions.AutoComplete;

class SearchQuotesAutoCompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter, IServiceProvider services)
    {
        if (autocompleteInteraction is not SocketAutocompleteInteraction interaction)
            return await Task.FromResult(
                AutocompletionResult.FromError(new ArgumentException("Not a SocketAutocompleteInteraction")));

        var query = (string) interaction.Data.Current.Value;
        var channel = interaction.Channel.Name;

        var dataService = services.GetRequiredService<IDataService>();

        
        return await FindSuggestionsAsync(query, channel, dataService);
    }

    //private async Task<AutocompletionResult> FindSuggestions(string query, string channel, IDataService dataService)
    //{
    //    var quotes = await dataService.FindQuotesByChannel(query, channel, 25).ConfigureAwait(false);
    //    var enumerable = quotes as Quoted[] ?? quotes.ToArray();
    //    if (!enumerable.Any())
    //    {
    //        return AutocompletionResult.FromSuccess();
    //    }

    //    List<AutocompleteResult> suggestions = new();
    //    foreach (var quote in enumerable)
    //    {
    //        string title = quote.CleanContent switch
    //        {
    //            { Length: > 100 } value => value.Substring(0, 90),
    //            { } value => value,
    //            _ => throw new ArgumentOutOfRangeException()
    //        };

    //        suggestions.Add(new AutocompleteResult(title, quote.Id));
    //    }
    //    // max - 25 suggestions at a time (API limit)
    //    return AutocompletionResult.FromSuccess(suggestions.Take(25));
    //}


    private async Task<AutocompletionResult> FindSuggestionsAsync(string query, string channel, IDataService dataService)
    {
        List<AutocompleteResult> suggestions = new();
        await foreach (var quote in dataService.FindQuotesByChannelAsync(query, channel, 25).ConfigureAwait(false))
        {
            string title = quote.CleanContent switch
            {
                { Length: > 100 } value => value.Substring(0, 100),
                { } value => value,
                _ => throw new ArgumentOutOfRangeException()
            };

            suggestions.Add(new AutocompleteResult(title, quote.Id));
        }
        // max - 25 suggestions at a time (API limit)
        return AutocompletionResult.FromSuccess(suggestions.Take(25));
    }

}