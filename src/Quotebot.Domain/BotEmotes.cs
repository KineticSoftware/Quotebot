namespace Quotebot.Domain;

public static class BotEmotes
{
    public const string QuotedRaw = "<:quoted:926362503531872317>";
    public static Emote QuotedEmote() => Emote.Parse(QuotedRaw);
}
