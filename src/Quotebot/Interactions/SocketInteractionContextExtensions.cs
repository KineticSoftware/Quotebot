using Discord.Interactions;


namespace Quotebot.Interactions;

internal static class SocketInteractionContextExtensions
{
    internal static async Task<IMessage> GetCompleteMessage(this SocketInteractionContext context, IMessage message) =>
        await context.Channel.GetMessageAsync(message.Id);

    internal static async Task<User> GetGuildUserName(this SocketInteractionContext context, IUser discordUser)
    {
        await context.Guild.DownloadUsersAsync();
        var user = context.Guild.GetUser(discordUser.Id);
        return new User(user);
    }
}
