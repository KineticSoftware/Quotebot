using Discord.Interactions;


namespace Quotebot.Interactions;

public static class SocketInteractionContextExtensions
{
    public static async Task<IMessage> GetCompleteMessage(this SocketInteractionContext context, IMessage message) =>
        await context.Channel.GetMessageAsync(message.Id);

    public static async Task<User> GetGuildUserName(this SocketInteractionContext context, IUser discordUser)
    {
        await context.Guild.DownloadUsersAsync();
        var user = context.Guild.GetUser(discordUser.Id);
        return new User(user);
    }
}
