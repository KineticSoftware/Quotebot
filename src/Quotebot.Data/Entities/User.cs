namespace Quotebot.Data.Entities;


public class User
{
    public User()
    { }

    public User(IUser user)
    {
        AvatarId = user.AvatarId;
        Discriminator = user.Discriminator;
        DiscriminatorValue = user.DiscriminatorValue;
        IsBot = user.IsBot;
        IsWebhook = user.IsWebhook;
        Username = user.Username;
        PublicFlags = user.PublicFlags;
        CreatedAt = user.CreatedAt;
        Id = user.Id;
        Mention = user.Mention;
        Status = user.Status;
        ActiveClients = user.ActiveClients;
        Activities = user.Activities;
    }

    public User(IGuildUser user)
    {
        AvatarId = user.AvatarId;
        Discriminator = user.Discriminator;
        DiscriminatorValue = user.DiscriminatorValue;
        IsBot = user.IsBot;
        IsWebhook = user.IsWebhook;
        Username = user.Username;
        Nickname = user.Nickname;
        PublicFlags = user.PublicFlags;
        CreatedAt = user.CreatedAt;
        Id = user.Id;
        Mention = user.Mention;
        Status = user.Status;
        ActiveClients = user.ActiveClients;
        Activities = user.Activities;
        JoinedAt = user.JoinedAt;
        GuildAvatarId = user.GuildAvatarId;
        GuildId = user.GuildId;
    }

    public string? AvatarId { get; set; }

    public string? Discriminator { get; set; }

    public ushort DiscriminatorValue { get; set; }

    public bool IsBot { get; set; }

    public bool IsWebhook { get; set; }

    public string? Username { get; set; }

    public string? Nickname { get; set; }

    public UserProperties? PublicFlags { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ulong Id { get; set; }

    public string? Mention { get; set; }

    public UserStatus Status { get; set; }

    public IReadOnlyCollection<ClientType>? ActiveClients { get; set; }

    public IReadOnlyCollection<IActivity>? Activities { get; set; }

    public DateTimeOffset? JoinedAt { get; set; }

    public string? GuildAvatarId { get; set; }

    public ulong GuildId { get; set; }
}
