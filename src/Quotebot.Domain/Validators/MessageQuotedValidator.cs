namespace Quotebot.Domain.Validators;

static class MessageQuotedValidator
{
    internal static bool Validate(this IMessage message, IUser quotedByUser, Func<string, Task> onValidationFailed)
    {
        if (message.Author.IsBot)
        {
            onValidationFailed($"{quotedByUser.Mention} sorry, you can't add quotes from bots.");
            return false;
        }
            
        if (string.IsNullOrWhiteSpace(message.CleanContent))
        {
            onValidationFailed($"{quotedByUser.Mention} no actual text was found. You can only quote text chat.");
            return false;
        }

        if (message.Embeds.Count > 0 || message.Attachments.Count > 0)
        {
            onValidationFailed($"{quotedByUser.Mention} an embed or an attachment was found. You can currently only quote text chat.");
            return false;
        }

        if (quotedByUser.Username == message.Author.Username || quotedByUser.Mention == message.Author.Mention)
        {
            onValidationFailed($"{quotedByUser.Mention} you're not allowed to quote yourself. ┗( T﹏T )┛");
            return false;
        }

        return true;
    }
}