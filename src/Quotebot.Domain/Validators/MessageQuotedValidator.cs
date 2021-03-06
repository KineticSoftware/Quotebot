namespace Quotebot.Domain.Validators
{
    public static class MessageQuotedValidator
    {
        public static (bool IsValid, string? validationException) Validate(this IMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.CleanContent))
            {
                return (false, "No actual text was found. You can only quote text chat.");
            }

            if (message.Embeds.Count > 0 || message.Attachments.Count > 0)
            {
                return (false, "An embed or an attachment was found. You can currently only quote text chat.");
            }

            return (true, null);
        }
    }
}
