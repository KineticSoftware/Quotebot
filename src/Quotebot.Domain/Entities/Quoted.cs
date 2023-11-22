using System.Text.Json.Serialization;

namespace Quotebot.Domain.Entities;

public class Quoted(string content)
{
    public Quoted(IMessage message) : this(message.Content)
    {
        Type = message.Type;
        Source = message.Source;
        MentionedEveryone = message.MentionedEveryone;
        CleanContent = message.CleanContent;
        Timestamp = message.Timestamp;
        EditedTimestamp = message.EditedTimestamp;
        Channel = new Channel(message.Channel);
        
        Author = message.Author switch
        {
            IGuildUser guildUser => new User(guildUser),
            _ => new User(message.Author)
        };

        Flags = message.Flags;
        CreatedAt = message.CreatedAt;
        Id = Convert.ToString(message.Id);

    }

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public MessageType Type { get; set; }

    public MessageSource Source { get; set; }

    public bool MentionedEveryone { get; set; }

    public string? Content { get; set; } = content;

    public string? CleanContent { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public DateTimeOffset? EditedTimestamp { get; set; }

    public Channel? Channel { get; set; }

    public User Author { get; set; } = new();

    public MessageFlags? Flags { get; set; }
}
