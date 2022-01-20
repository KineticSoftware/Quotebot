namespace Quotebot.Data.Entities;

public class Quoted
{
    public Quoted()
    { }

    public Quoted(IMessage message)
    {
        Type = message.Type;
        Source = message.Source;
        MentionedEveryone = message.MentionedEveryone;
        Content = message.Content;
        CleanContent = message.CleanContent;
        Timestamp = message.Timestamp;
        EditedTimestamp = message.EditedTimestamp;
        Channel = new Channel(message.Channel);
        Author = new User(message.Author);
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

    public string? Content { get; set; }

    public string? CleanContent { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public DateTimeOffset? EditedTimestamp { get; set; }

    public Channel Channel { get; set; } = new();

    public User Author { get; set; } = new();

    public MessageFlags? Flags { get; set; }
}
