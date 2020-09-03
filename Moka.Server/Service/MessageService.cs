using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Moka.Server.Data;
using Moka.Server.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Moka.Server.Service
{
    public interface IMessageService
    {
        Task<MessageData> Store(Message message);
        Task<MessageData> Find(Message message);
        Task<List<MessageData>> FindUserInboxMessages(UserData user);
        Task UpdateAck(UserModel user, MessageAck ack);

    }

    public class MessageService : IMessageService
    {
        private readonly IMongoCollection<MessageData> _messages;
        private readonly ILogger<MessageService> _logger;

        public MessageService(IMokaDataBaseSettings settings, ILogger<MessageService> logger)
        {
            _logger = logger;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _messages = database.GetCollection<MessageData>(settings.MessagesCollectionName);
        }
        public async Task<List<MessageData>> FindUserInboxMessages(UserData user)
        {
            var res = await _messages.FindAsync(m => m.To == user.Guid);
            return await res.ToListAsync();
        }
        public async Task<MessageData> Store(Message message)
        {
            _logger.LogInformation("storing message");
            message.CreatedAtTimeStamp = Timestamp.FromDateTime(DateTime.UtcNow);
            var msg = MessageData.FromMessage(message);
            await _messages.InsertOneAsync(msg);
            return msg;
            
        }

        public async Task<MessageData> Find(Message message)
        {
            var res = await _messages.FindAsync(m => m.Id == ObjectId.Parse(message.Id));
            return await res.FirstOrDefaultAsync();
        }

        public async Task<MessageData> Find(Guid localId)
        {
            var res = await _messages.FindAsync(m => m.LocalId == localId);
            return await res.FirstOrDefaultAsync();
        }

        public async Task UpdateAck(UserModel user,MessageAck ack)
        {
            switch (ack.AckType)
            {
                case AckType.Deliver:
                    var condDeliver = Builders<MessageData>.Filter.Eq("To", user.Guid.ToString()) & Builders<MessageData>.Filter.Eq("Delivered_at", default(DateTime));
                    var updateDeliver = Builders<MessageData>.Update.Set("Delivered_at", ack.TimeStamp.ToDateTime());
                    var result = await _messages.UpdateManyAsync(condDeliver,updateDeliver);
                    _logger.LogInformation("marked as delv: "+result.ModifiedCount);
                    break;
                case AckType.Read:
                    var condRead = Builders<MessageData>.Filter.Eq("From", ack.UserId) & Builders<MessageData>.Filter.Eq("To", user.Guid.ToString()) & Builders<MessageData>.Filter.Eq("Read_at", default(DateTime)) & Builders<MessageData>.Filter.Eq("Read_at", default(DateTime));
                    var updateRead = Builders<MessageData>.Update.Set("Read_at", ack.TimeStamp.ToDateTime());
                    await _messages.UpdateManyAsync(condRead,updateRead);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}