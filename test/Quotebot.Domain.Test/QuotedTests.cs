using System.Formats.Tar;
using Discord;
using Discord.Mocks;
using Quotebot.Domain.Entities;

namespace Quotebot.Domain.Test;

public class QuotedTests
{
    [Fact]
    public void Quoted_Should_Always_Set_Content_With_String()
    {
        string expected = "test";
        Quoted test = new(expected);
        Assert.Equal(expected, test.Content);
    }

    [Fact]
    public void Quoted_Should_Always_Set_Content_With_IMessage()
    {
        MessageMock messageMock= new();

        string expectedContent = "test";
        messageMock.Setup.Content.Returns(expectedContent);

        MessageType expectMessageType = MessageType.StageRaiseHand;
        messageMock.Setup.Type.Returns(expectMessageType);

        MessageSource expectMessageSource = MessageSource.User;
        messageMock.Setup.Source.Returns(expectMessageSource);

        bool expectMentionedEveryone = true;
        messageMock.Setup.MentionedEveryone.Returns(expectMentionedEveryone);

        string expectedCleanContent = "clean-content";
        messageMock.Setup.CleanContent.Returns(expectedCleanContent);

        DateTimeOffset expectDateTimeOffset = DateTimeOffset.Now.Date;
        messageMock.Setup.Timestamp.Returns(expectDateTimeOffset);

        DateTimeOffset expectEditedDateTimeOffset = DateTimeOffset.Now.Date.Add(new TimeSpan(1, 0, 0));
        messageMock.Setup.EditedTimestamp.Returns(expectEditedDateTimeOffset);

        MessageChannelMock messageChannelMock = new();
        string expectedChannelName = "test-channel";
        messageChannelMock.Setup.Name.Returns(expectedChannelName);
        messageMock.Setup.Channel.Returns(messageChannelMock);

        UserMock userMock = new();
        string expectedAuthorUsername = "test-user";
        userMock.Setup.Username.Returns(expectedAuthorUsername);
        messageMock.Setup.Author.Returns(userMock);

        MessageFlags expectedFlags = MessageFlags.Urgent | MessageFlags.Ephemeral;
        messageMock.Setup.Flags.Returns(expectedFlags);

        messageMock.Setup.CreatedAt.Returns(expectDateTimeOffset);

        ulong expectedId = 1337;
        messageMock.Setup.Id.Returns(expectedId);

        Quoted test = new(messageMock);
        
        Assert.Equal(Convert.ToString(expectedId), test.Id);
        Assert.Equal(expectDateTimeOffset, test.CreatedAt);
        Assert.Equal(expectMessageType, test.Type);
        Assert.Equal(expectMessageSource, test.Source);
        Assert.Equal(expectMentionedEveryone, test.MentionedEveryone);
        Assert.Equal(expectedContent, test.Content);
        Assert.Equal(expectedCleanContent, test.CleanContent);
        Assert.Equal(expectDateTimeOffset, test.Timestamp);
        Assert.Equal(expectEditedDateTimeOffset, test.EditedTimestamp);
        Assert.NotNull(test.Channel);
        Assert.Equal(expectedChannelName, test.Channel?.Name);
        Assert.Equal(expectedAuthorUsername, test.Author.Username);
        Assert.Equal(expectedFlags, test.Flags);
    }
}