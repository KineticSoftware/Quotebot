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
        
        var user = await context.Guild.GetUserAsync(userId);

        var dataService = services.GetRequiredService<IDataService>();


        List<AutocompleteResult> suggestions = new();
        IEnumerable<Quoted> quotes = await dataService.FindQuotesByUserInChannel(user, channel, query);
        foreach (var quote in quotes)
        {
            suggestions.Add(new AutocompleteResult($"{quote.CleanContent}", $"{quote.CreatedAt:d} - **{quote.Author.Nickname ?? quote.Author.Username}** : {quote.Content}"));
        }

        // max - 25 suggestions at a time (API limit)
        return AutocompletionResult.FromSuccess(suggestions.Take(25));
    }
}