using Discord.Mocks;
using Quotebot.Domain.Entities;

namespace Quotebot.Domain.Test;

public class ChannelTests
{
    [Fact]
    public void Channel_Ctor_With_Parameters_Should_Assign_Properties()
    {
        ChannelMock mock = new();
        string expectedName = "test_user";
        mock.Setup.Name.Returns(expectedName);
        
        DateTimeOffset expectDateTimeOffset = new(DateTime.Now.Date);
        mock.Setup.CreatedAt.Returns(expectDateTimeOffset);

        ulong expectedChannelId = 1337;
        mock.Setup.Id.Returns(expectedChannelId);

        Channel test = new(mock);

        Assert.Equal(expectedName, test.Name);
        Assert.Equal(expectDateTimeOffset, test.CreatedAt);
        Assert.Equal(expectedChannelId, test.Id);
    }

}