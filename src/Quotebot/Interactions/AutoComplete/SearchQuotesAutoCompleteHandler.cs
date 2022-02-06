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
        var rows = await dataService.FindQuotesByUserInChannel(user, channel, query);
        foreach (var row in rows)
        {
            suggestions.Add(new AutocompleteResult(row.CleanContent, row.Content));
        }

        return await Task.FromResult(AutocompletionResult.FromSuccess(suggestions));
    }
}