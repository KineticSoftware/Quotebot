using Discord.Interactions;
using System.Text.Json;

namespace Quotebot.Interactions.AutoComplete;

class SearchQuotesAutoCompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter, IServiceProvider services)
    {
        if (autocompleteInteraction is not SocketAutocompleteInteraction interaction)
            return await Task.FromResult(AutocompletionResult.FromError(new ArgumentException("Not a SocketAutocompleteInteraction")));

        var query = (string)interaction.Data.Current.Value;
        var channel = interaction.Channel.Name;
        ulong userId = Convert.ToUInt64(interaction.Data.Options.FirstOrDefault(x => x.Name == "user")?.Value);
        
        var dataService = services.GetRequiredService<IDataService>();


        List<AutocompleteResult> suggestions = new();

        // max - 25 suggestions at a time (API limit)
        var quotes = await dataService.FindQuotesByChannel(query, channel, 25);
        foreach (var quote in quotes)
        {
            string title = quote.CleanContent switch
            {
                {Length: > 100} value => value.Substring(0, 90),
                { } value => value,
                _ => throw new ArgumentOutOfRangeException()
            };

            suggestions.Add(new AutocompleteResult(title, quote.Id));
        }

        return AutocompletionResult.FromSuccess(suggestions.Take(25));
    }
}