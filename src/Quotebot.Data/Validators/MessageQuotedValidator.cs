using Discord;

namespace Quotebot.Data.Validators;

static class MessageQuotedValidator
{
    internal static bool Validate(this IMessage message, IUser quotedByUser, Func<string, Task> onValidationFailed)
    {
        string? failureReason = message switch
        {
            _ when message.Author.IsBot => $"{quotedByUser.Mention} sorry, you can't add quotes from bots.",
            _ when string.IsNullOrWhiteSpace(message.CleanContent) => $"{quotedByUser.Mention} no actual text was found. You can only quote text chat.",
            _ when message.Embeds.Count > 0 || message.Attachments.Count > 0 => $"{quotedByUser.Mention} an embed or an attachment was found. You can currently only quote text chat.",
            _ when quotedByUser.Username == message.Author.Username || quotedByUser.Mention == message.Author.Mention => $"{quotedByUser.Mention} you're not allowed to quote yourself. ┗( T﹏T )┛",
            _ => null
        };

        if (failureReason != null)
        {
            onValidationFailed(failureReason);
            return false;
        }

        return true;
    }

}