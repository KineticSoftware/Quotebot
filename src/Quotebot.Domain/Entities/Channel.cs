namespace Quotebot.Domain.Entities;

public class Channel
{
    public Channel(IChannel channel)
    {
        Name = channel.Name;
        CreatedAt = channel.CreatedAt;
        Id = channel.Id;
    }
    public string? Name { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ulong Id { get; set; }
}
