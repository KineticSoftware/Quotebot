using Discord.Commands;

namespace Quotebot.Commands;

internal static class SocketCommandContextExtensions
{
    internal static async Task<IMessage> GetCompleteMessage(this SocketCommandContext context, IMessage message) =>
        await context.Channel.GetMessageAsync(message.Id);

    internal static async Task<User> GetGuildUserName(this SocketCommandContext context, IUser discordUser)
    {
        await context.Guild.DownloadUsersAsync();
        var user = context.Guild.GetUser(discordUser.Id);
        return new User(user);
    }
}