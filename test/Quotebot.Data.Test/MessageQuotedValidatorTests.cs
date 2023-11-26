using System.Collections.ObjectModel;
using Discord;
using Discord.Mocks;
using Quotebot.Data.Validators;

namespace Quotebot.Data.Test;

public class MessageQuotedValidatorTests
{
    [Fact]
    public void Validate_Should_Return_True_Happy_Path()
    {
        MessageMock messageMock = new();
        UserMock authorMock = new();
        authorMock.Setup.IsBot.Returns(false);
        authorMock.Setup.Username.Returns("quoted");
        authorMock.Setup.Mention.Returns("@quoted");
        messageMock.Setup.Author.Returns(authorMock);
        messageMock.Setup.CleanContent.Returns("Test");

        UserMock quotedByUserMock = new();
        quotedByUserMock.Setup.Username.Returns("quoted-by");
        quotedByUserMock.Setup.Mention.Returns("@quoted-by");

        string actualValidation = string.Empty;
        bool actual = messageMock.Validate(quotedByUserMock, async validationMessage =>
        {
            actualValidation = validationMessage;
            await Task.CompletedTask;
        });

        Assert.True(actual);
        Assert.Empty(actualValidation);
    }

    [Fact]
    public void Validate_Should_Return_False_When_Author_IsBot()
    {
        MessageMock messageMock = new();
        UserMock authorMock = new();
        authorMock.Setup.IsBot.Returns(true);
        messageMock.Setup.Author.Returns(authorMock);

        UserMock quotedByUserMock = new();
        quotedByUserMock.Setup.Mention.Returns("@quoted-by");

        string actualValidation = string.Empty;
        bool actual = messageMock.Validate(quotedByUserMock, async validationMessage =>
        {
            actualValidation = validationMessage;
            await Task.CompletedTask;
        });

        Assert.False(actual);
        Assert.Equal($"{quotedByUserMock.Mention} sorry, you can't add quotes from bots.", actualValidation);
    }

    [Fact]
    public void Validate_Should_Return_False_When_CleanContent_Is_WhiteSpace()
    {
        MessageMock messageMock = new();
        UserMock authorMock = new();
        messageMock.Setup.Author.Returns(authorMock);

        messageMock.Setup.CleanContent.Returns(" ");

        UserMock quotedByUserMock = new();
        quotedByUserMock.Setup.Mention.Returns("@quoted-by");

        string actualValidation = string.Empty;
        bool actual = messageMock.Validate(quotedByUserMock, async validationMessage =>
        {
            actualValidation = validationMessage;
            await Task.CompletedTask;
        });

        Assert.False(actual);
        Assert.Equal($"{quotedByUserMock.Mention} no actual text was found. You can only quote text chat.", actualValidation);
    }

    [Fact]
    public void Validate_Should_Return_False_When_Embeds_Count_Greater_Than_Zero()
    {
        MessageMock messageMock = new();
        UserMock authorMock = new();
        messageMock.Setup.Author.Returns(authorMock);

        messageMock.Setup.CleanContent.Returns("test");
        EmbedMock embedMock = new();
        embedMock.Setup.Type.Returns(EmbedType.Video);
        embedMock.Setup.Video.Returns(new EmbedVideo());
        messageMock.Setup.Embeds.Returns(new ReadOnlyCollection<IEmbed>(new List<IEmbed> {embedMock}));

        UserMock quotedByUserMock = new();
        quotedByUserMock.Setup.Mention.Returns("@quoted-by");

        string actualValidation = string.Empty;
        bool actual = messageMock.Validate(quotedByUserMock, async validationMessage =>
        {
            actualValidation = validationMessage;
            await Task.CompletedTask;
        });

        Assert.False(actual);
        Assert.Equal($"{quotedByUserMock.Mention} an embed or an attachment was found. You can currently only quote text chat.", actualValidation);
    }

    [Fact]
    public void Validate_Should_Return_False_When_Attachment_Count_Greater_Than_Zero()
    {
        MessageMock messageMock = new();
        UserMock authorMock = new();
        messageMock.Setup.Author.Returns(authorMock);

        messageMock.Setup.CleanContent.Returns("test");
        AttachmentMock attachmentMock = new();
        attachmentMock.Setup.Filename.Returns("test.jpg");
        messageMock.Setup.Attachments.Returns(new ReadOnlyCollection<IAttachment>(new List<IAttachment> {attachmentMock}));

        UserMock quotedByUserMock = new();
        quotedByUserMock.Setup.Mention.Returns("@quoted-by");

        string actualValidation = string.Empty;
        bool actual = messageMock.Validate(quotedByUserMock, async validationMessage =>
        {
            actualValidation = validationMessage;
            await Task.CompletedTask;
        });

        Assert.False(actual);
        Assert.Equal($"{quotedByUserMock.Mention} an embed or an attachment was found. You can currently only quote text chat.", actualValidation);
    }

    [Fact]
    public void Validate_Should_Return_False_When_QuotedBy_Username_Equals_Author_UserName()
    {
        MessageMock messageMock = new();
        UserMock authorMock = new();
        authorMock.Setup.IsBot.Returns(false);
        authorMock.Setup.Username.Returns("quoted");
        messageMock.Setup.Author.Returns(authorMock);

        messageMock.Setup.CleanContent.Returns("test");

        UserMock quotedByUserMock = new();
        quotedByUserMock.Setup.Username.Returns("quoted");

        string actualValidation = string.Empty;
        bool actual = messageMock.Validate(quotedByUserMock, async validationMessage =>
        {
            actualValidation = validationMessage;
            await Task.CompletedTask;
        });

        Assert.False(actual);
        Assert.Equal($"{quotedByUserMock.Mention} you're not allowed to quote yourself. ┗( T﹏T )┛", actualValidation);
    }

    [Fact]
    public void Validate_Should_Return_False_When_QuotedBy_Mention_Equals_Author_Mention()
    {
        MessageMock messageMock = new();
        UserMock authorMock = new();
        authorMock.Setup.IsBot.Returns(false);
        authorMock.Setup.Username.Returns("quoted");
        authorMock.Setup.Mention.Returns("@quoted");
        messageMock.Setup.Author.Returns(authorMock);

        messageMock.Setup.CleanContent.Returns("test");

        UserMock quotedByUserMock = new();
        quotedByUserMock.Setup.Username.Returns("quoted-by");
        quotedByUserMock.Setup.Mention.Returns("@quoted");

        string actualValidation = string.Empty;
        bool actual = messageMock.Validate(quotedByUserMock, async validationMessage =>
        {
            actualValidation = validationMessage;
            await Task.CompletedTask;
        });

        Assert.False(actual);
        Assert.Equal($"{quotedByUserMock.Mention} you're not allowed to quote yourself. ┗( T﹏T )┛", actualValidation);
    }
}