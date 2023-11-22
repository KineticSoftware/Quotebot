namespace Quotebot.Domain.Test;

public class BotEmoteTests
{
    [Fact]
    public void QuotedRaw_Should_Be_Expected_Value()
    {
        Assert.Equal("<:quoted:926362503531872317>", BotEmotes.QuotedRaw);
    }

    [Fact]
    public void QuotedEmote_Should_Not_Be_Null()
    {
        Assert.NotNull(BotEmotes.QuotedEmote());
    }
}