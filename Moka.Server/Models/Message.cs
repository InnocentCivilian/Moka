using System;

namespace Moka.Server.Models
{
    public class Message
    {
        public Guid Guid { get; set; }
        public byte[] Data { get; set; }
        public MessageType MessageType { get; set; }
        public User From { get; set; }
        public User To { get; set; }
        public DateTime Created_at { get; set; }

        public Message(Guid guid, byte[] data, MessageType messageType, User @from, User to, DateTime createdAt)
        {
            Guid = guid;
            Data = data;
            MessageType = messageType;
            From = @from;
            To = to;
            Created_at = createdAt;
        }
    }
}