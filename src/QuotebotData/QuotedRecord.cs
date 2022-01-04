using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Quotebot.Data
{
    internal class QuotedCosmosRecord : IMessage
    {
        public QuotedCosmosRecord(IMessage message)
        {
            Type = message.Type;
            Source = message.Source;
            IsTTS = message.IsTTS; 
            IsPinned = message.IsPinned;
            IsSuppressed = message.IsSuppressed;
            MentionedEveryone = message.MentionedEveryone;
            Content = message.Content;
            CleanContent = message.CleanContent;
            Timestamp = message.Timestamp;
            EditedTimestamp = message.EditedTimestamp;
            Channel = message.Channel;
            Author = message.Author;
            Attachments = message.Attachments;
            Embeds = message.Embeds;
            Tags = message.Tags;
            MentionedChannelIds = message.MentionedChannelIds;
            MentionedRoleIds = message.MentionedRoleIds;
            MentionedUserIds = message.MentionedUserIds;
            Activity = message.Activity;
            Application = message.Application;
            Reference = message.Reference;
            Reactions = message.Reactions;
            Components = message.Components;
            Stickers = message.Stickers;
            Flags = message.Flags;
            Interaction = message.Interaction;
            CreatedAt = message.CreatedAt;
            Id = message.Id;

        }

        public MessageType Type { get; init; }

        public MessageSource Source { get; init; }

        public bool IsTTS { get; init; }

        public bool IsPinned { get; init; }

        public bool IsSuppressed { get; init; }

        public bool MentionedEveryone { get; init; }

        public string Content { get; init; }

        public string CleanContent { get; init; }

        public DateTimeOffset Timestamp { get; init; }

        public DateTimeOffset? EditedTimestamp { get; init; }

        public IMessageChannel Channel { get; init; }

        public IUser Author { get; init; }

        [JsonIgnore]
        public IReadOnlyCollection<IAttachment> Attachments { get; init; }

        [JsonIgnore]
        public IReadOnlyCollection<IEmbed> Embeds { get; init; }


        public IReadOnlyCollection<ITag> Tags { get; init; }

        public IReadOnlyCollection<ulong> MentionedChannelIds { get; init; }

        public IReadOnlyCollection<ulong> MentionedRoleIds { get; init; }

        public IReadOnlyCollection<ulong> MentionedUserIds { get; init; }

        public MessageActivity Activity { get; init; }

        public MessageApplication Application { get; init; }

        public MessageReference Reference { get; init; }

        [JsonIgnore]
        public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions { get; init; }

        public IReadOnlyCollection<IMessageComponent> Components { get; init; }

        public IReadOnlyCollection<IStickerItem> Stickers { get; init; }

        public MessageFlags? Flags { get; init; }

        public IMessageInteraction Interaction { get; init; }

        public DateTimeOffset CreatedAt { get; init; }

        public ulong Id { get; init; }

        public Task AddReactionAsync(IEmote emote, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emoji, int limit, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAllReactionsAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAllReactionsForEmoteAsync(IEmote emote, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveReactionAsync(IEmote emote, IUser user, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveReactionAsync(IEmote emote, ulong userId, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }
    }
}
