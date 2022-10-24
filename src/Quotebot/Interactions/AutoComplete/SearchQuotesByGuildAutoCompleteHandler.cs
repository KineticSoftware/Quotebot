using Discord.Interactions;

namespace Quotebot.Interactions.AutoComplete;

class SearchQuotesByGuildAutoCompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter, IServiceProvider services)
    {
        if (autocompleteInteraction is not SocketAutocompleteInteraction interaction)
            return await Task.FromResult(
                AutocompletionResult.FromError(new ArgumentException("Not a SocketAutocompleteInteraction")));

        var query = (string) interaction.Data.Current.Value;

        var dataService = services.GetRequiredService<IDataService>();
        
        return await FindSuggestionsAsync(query, dataService);
    }

    private async Task<AutocompletionResult> FindSuggestionsAsync(string query, IDataService dataService)
    {
        List<AutocompleteResult> suggestions = new();
        await foreach (var quote in dataService.FindQuotesByGuildAsync(query).ConfigureAwait(false))
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