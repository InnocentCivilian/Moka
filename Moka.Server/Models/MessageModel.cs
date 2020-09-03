// using System;
// using Google.Protobuf;
// using Google.Protobuf.WellKnownTypes;
//
// namespace Moka.Server.Models
// {
//     public class MessageModel
//     {
//         public Guid Guid { get; set; }
//         public Guid LocalId { get; set; }
//         public byte[] Data { get; set; }
//         public MessageModelType MessageType { get; set; }
//         public UserModel From { get; set; }
//         public UserModel To { get; set; }
//         public DateTime Created_at { get; set; }
//
//         public MessageModel(Guid guid, byte[] data, MessageModelType messageType, UserModel from, UserModel to,
//             DateTime createdAt)
//         {
//             Console.WriteLine("hi mf");
//             Guid = guid;
//             Data = data;
//             MessageType = messageType;
//             From = from;
//             To = to;
//             Created_at = createdAt;
//         }
//
//         public static MessageModel CreateNew(Guid guid, byte[] toByteArray, MessageModelType text,
//             UserModel toUserModel, UserModel createdAt, in DateTime now)
//         {
//             Console.WriteLine("creating");
//             return new MessageModel(guid, toByteArray, text, toUserModel, createdAt, now);
//         }
//
//         public Message ToMessage()
//         {
//             return new Message
//             {
//                 CreatedAtTimeStamp = Timestamp.FromDateTime(Created_at),
//                 Id = Guid.ToString(),
//                 LocalId = LocalId.ToString(),
//                 Payload = ByteString.CopyFrom(Data),
//                 ReceiverId = To.Guid.ToString(),
//                 SenderId = From.Guid.ToString(),
//                 Type = global::MessageType.Text
//             };
//         }
//     }
// }