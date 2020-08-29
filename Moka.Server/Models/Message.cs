using System;

namespace Moka.Server.Models
{
    public class Message
    {
        public Guid Guid { get; set; }
        public byte[] Data { get; set; }
        public MessageType MessageType { get; set; }
        public UserModel From { get; set; }
        public UserModel To { get; set; }
        public DateTime Created_at { get; set; }

        public Message(Guid guid, byte[] data, MessageType messageType, UserModel @from, UserModel to, DateTime createdAt)
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