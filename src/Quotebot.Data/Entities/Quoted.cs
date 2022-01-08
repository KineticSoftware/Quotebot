﻿using Discord;
using System.Text.Json.Serialization;

namespace Quotebot.Data.Entities
{
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
            Reference = message.Reference;
            Flags = message.Flags;
            CreatedAt = message.CreatedAt;
            DiscordMessageId = message.Id;
            Id = Guid.NewGuid();

        }

        public MessageType Type { get; set; }

        public MessageSource Source { get; set; }


        public bool MentionedEveryone { get; set; }

        public string? Content { get; set; }

        public string? CleanContent { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public DateTimeOffset? EditedTimestamp { get; set; }

        public Channel? Channel { get; set; }

        public User? Author { get; set; }

        public MessageReference? Reference { get; set; }

        public MessageFlags? Flags { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public ulong DiscordMessageId { get; set; }

        [JsonPropertyName("id")]
        public Guid Id { get; set; }

    }
}