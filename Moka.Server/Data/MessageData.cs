using System;
using Moka.Server.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Moka.Server.Data
{
    public class MessageData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public Guid Guid { get; set; }

        public byte[] Data { get; set; }
        public MessageModelType MessageType { get; set; }
        public UserData From { get; set; }
        public UserData To { get; set; }
        public DateTime Created_at { get; set; }

        public MessageData(Guid guid, byte[] data, MessageModelType messageType, UserData from, UserData to, DateTime createdAt)
        {
            Guid = guid;
            Data = data;
            MessageType = messageType;
            From = from;
            To = to;
            Created_at = createdAt;
        }

        public static MessageData FromMessage(MessageModel Message)
        {
            return new MessageData(Message.Guid, Message.Data, Message.MessageType, UserData.FromUser(Message.From),
                UserData.FromUser(Message.To), Message.Created_at);
        }

        public MessageModel ToMessage()
        {
            return new MessageModel(Guid,Data,MessageType,From.ToUserModel(),To.ToUserModel(),Created_at);
        }
    }
}