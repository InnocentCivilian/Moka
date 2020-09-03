using System;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Moka.Server.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Moka.Server.Data
{
    public class MessageData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        public Guid LocalId { get; set; }


        public byte[] Data { get; set; }
        public MessageType MessageType { get; set; }
        public Guid From { get; set; }
        public Guid To { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime? Delivered_at { get; set; }
        public DateTime? Read_at { get; set; }


        public MessageData(ObjectId id, Guid localId, byte[] data, MessageType messageType, Guid @from, Guid to,
            DateTime createdAt, DateTime deliveredAt = default, DateTime readAt = default)
        {
            Id = id;
            LocalId = localId;
            Data = data;
            MessageType = messageType;
            From = @from;
            To = to;
            Created_at = createdAt;
            Delivered_at = deliveredAt;
            Read_at = readAt;
        }

        public static MessageData FromMessage(Message msg)
        {
            ObjectId id;
            if (!ObjectId.TryParse(msg.Id, out id
            ))
            {
                id = ObjectId.GenerateNewId();
            }

            return new MessageData(
                id,
                Guid.Parse(msg.LocalId),
                msg.Payload.ToByteArray(),
                msg.Type,
                Guid.Parse(msg.SenderId),
                Guid.Parse(msg.ReceiverId),
                msg.CreatedAtTimeStamp.ToDateTime()
            );
        }

        public Message ToMessage()
        {
            var msg = new Message
            {
                CreatedAtTimeStamp = Timestamp.FromDateTime(Created_at),
                Id = Id.ToString(),
                LocalId = LocalId.ToString(),
                Payload = ByteString.CopyFrom(Data),
                ReceiverId = To.ToString(),
                SenderId = From.ToString(),
                Type = MessageType,
            };
            if (!Delivered_at.HasValue) return msg;
            msg.DeliveredAtTimeStamp =
                Timestamp.FromDateTime(Delivered_at.Value.ToUniversalTime());
            msg.ReadAtTimeStamp =
                Read_at.HasValue ? Timestamp.FromDateTime(Read_at.Value.ToUniversalTime()) : null;

            return msg;
        }
    }
}