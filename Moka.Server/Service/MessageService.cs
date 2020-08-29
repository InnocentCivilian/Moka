﻿// using System;
// using System.Threading.Tasks;
// using Moka.Server.Data;
// using Moka.Server.Models;
// using MongoDB.Driver;
//
// namespace Moka.Server.Service
// {
//     public class MessageService
//     {
//         private readonly IMongoCollection<MessageData> _messages;
//
//         public MessageService(IMokaDataBaseSettings settings)
//         {
//             var client = new MongoClient(settings.ConnectionString);
//             var database = client.GetDatabase(settings.DatabaseName);
//             _messages = database.GetCollection<MessageData>(settings.MessagesCollectionName);
//         }
//
//         public async Task<StoreMessageResult> Store(Message message)
//         {
//             message.Guid = Guid.NewGuid();
//             await _messages.InsertOneAsync(MessageData.FromMessage(message));
//             var stored = await Find(message);
//             return new StoreMessageResult(true, message.Created_at, stored.ToMessage());
//         }
//
//         private async Task<MessageData> Find(Message message)
//         {
//             var res = await _messages.FindAsync(m => m.Guid == message.Guid);
//             return await res.FirstOrDefaultAsync();
//         }
//     }
//
//     public class StoreMessageResult
//     {
//         public bool isSuccess { get; set; }
//         public DateTime CreateDateTime { get; set; }
//         public Message Message { get; set; }
//
//         public StoreMessageResult(bool isSuccess, DateTime dateTime, Message message)
//         {
//             this.isSuccess = isSuccess;
//             CreateDateTime = dateTime;
//             Message = message;
//         }
//     }
// }