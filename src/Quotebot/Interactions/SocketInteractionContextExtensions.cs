using Discord.Interactions;
using Quotebot.Data.Entities;

namespace Quotebot.Interactions
{
    public static class SocketInteractionContextExtensions
    {
        public async static Task<IMessage> GetCompleteMessage(this SocketInteractionContext context, IMessage message) => 
            await context.Channel.GetMessageAsync(message.Id);

        public async static Task<IMessage?>? GetLatestBotMessageAsync(this SocketInteractionContext context, ulong botId = 925598461695524964) =>
            (await context.GetLatestChannelMessagesAsync())
                .Where(message => message?.Author?.IsBot ?? false && message?.Author?.Id == botId)
                .OrderByDescending(message => message?.CreatedAt)
                .FirstOrDefault();
            
        public async static Task<IEnumerable<IMessage>> GetLatestChannelMessagesAsync(this SocketInteractionContext context, int count = 50) =>
            await context.Channel.GetMessagesAsync(count).FlattenAsync();

        public static User GetGuildUserName(this SocketInteractionContext context, IUser discordUser) {
            var user = context.Guild.GetUser(discordUser.Id);
            return new User(user);
        }
    }
}
