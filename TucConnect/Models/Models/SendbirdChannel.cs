using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TucConnect.Models.Models
{

    public class SendbirdMessageEvent
    {
        public string SendPushNotification { get; set; }
        public bool UpdateUnreadCount { get; set; }
        public bool UpdateMentionCount { get; set; }
        public bool UpdateLastMessage { get; set; }
    }

    public class SendbirdResponse
    {
        [JsonPropertyName("messages")]
        public List<SendbirdMensaje> Messages { get; set; }
    }

    public class SendbirdMensaje
    {
        [JsonPropertyName("message_id")]
        public long MessageId { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("user")]
        public SendbirdMember User { get; set; }

        [JsonPropertyName("channel_url")]
        public string ChannelUrl { get; set; }

        [JsonPropertyName("created_at")]
        public long CreatedAt { get; set; }
    }

    public class SendbirdMember
    {
        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }

        [JsonPropertyName("nickname")]
        public string? Nickname { get; set; }

        // Agrega cualquier otra propiedad necesaria
    }

    public class SendbirdLastMessage
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("message_id")]
        public long MessageId { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        // Agrega cualquier otra propiedad necesaria
    }

    public class SendbirdChannel
    {
        [JsonPropertyName("channel_url")]
        public string? ChannelUrl { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("cover_url")]
        public string? CoverUrl { get; set; }

        [JsonPropertyName("member_count")]
        public int MemberCount { get; set; }

        [JsonPropertyName("joined_member_count")]
        public int JoinedMemberCount { get; set; }

        [JsonPropertyName("created_at")]
        public long CreatedAt { get; set; }

        [JsonPropertyName("members")]
        public List<SendbirdMember>? Members { get; set; }

        [JsonPropertyName("last_message")]
        public SendbirdLastMessage? LastMessage { get; set; }



        // Agrega cualquier otra propiedad necesaria
    }

    public class SendbirdChannelsResponse
    {
        [JsonPropertyName("channels")]
        public List<SendbirdChannel>? Channels { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("ts")]
        public long Ts { get; set; }
    }
}
