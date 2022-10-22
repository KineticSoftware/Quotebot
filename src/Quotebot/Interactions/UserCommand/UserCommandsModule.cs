using Discord.Interactions;

namespace Quotebot.Interactions.UserCommand;

// ReSharper disable once UnusedType.Global
public class UserCommandsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDataService _dataService;

    public UserCommandsModule(IDataService dataService)
    {
        _dataService = dataService;
    }

    [UserCommand("Count of Quotes")]
    public async Task GetUserQuoteCount(IUser user)
    {
        await DeferAsync();
        var guildUser = await Context.GetGuildUserName(user);
        var countedQuotes = await _dataService.QuotesCountByUser(guildUser);

        var respsonse = countedQuotes switch
        {
            0 => $"{guildUser.Nickname ?? guildUser.Username} has never been quoted.",
            1 => $"{guildUser.Nickname ?? guildUser.Username} has been quoted once.",
            _ => $"{guildUser.Nickname ?? guildUser.Username} has been quoted {countedQuotes} times."
        };

        await FollowupAsync(respsonse);
    }
}
