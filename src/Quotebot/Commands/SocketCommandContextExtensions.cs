using Discord.Commands;

namespace Quotebot.Commands;

public static class SocketCommandContextExtensions
{
    public static async Task<IMessage> GetCompleteMessage(this SocketCommandContext context, IMessage message) =>
        await context.Channel.GetMessageAsync(message.Id);

    public static async Task<User> GetGuildUserName(this SocketCommandContext context, IUser discordUser)
    {
        await context.Guild.DownloadUsersAsync();
        var user = context.Guild.GetUser(discordUser.Id);
        return new User(user);
    }
}