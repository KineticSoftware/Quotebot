using Discord;

namespace Quotebot.Data.Entities
{
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
        public string? AvatarId { get; set; }

        public string? Discriminator { get; set; }

        public ushort DiscriminatorValue { get; set; }

        public bool IsBot { get; set; }

        public bool IsWebhook { get; set; }

        public string? Username { get; set; }

        public UserProperties? PublicFlags { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public ulong Id { get; set; }

        public string? Mention { get; set; }

        public UserStatus Status { get; set; }

        public IReadOnlyCollection<ClientType>? ActiveClients { get; set; }

        public IReadOnlyCollection<IActivity>? Activities { get; set; }
    }
}
