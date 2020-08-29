using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moka.Server.Data;
using Moka.Server.Models;
using MongoDB.Driver;

namespace Moka.Server.Service
{
    public interface IMessageService
    {
        Task<MessageData> Store(MessageModel message);
        Task<MessageData> Find(MessageModel message);
        Task sayHi();
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

        public async Task<MessageData> Store(MessageModel message)
        {
            _logger.LogInformation("storing message");
            message.Guid = Guid.NewGuid();
            await _messages.InsertOneAsync(MessageData.FromMessage(message));
            var stored = await Find(message);
            return stored;
            // return new StoreMessageResult(true, message.Created_at, stored.ToMessage());
        }

        public async Task<MessageData> Find(MessageModel message)
        {
            var res = await _messages.FindAsync(m => m.Guid == message.Guid);
            return await res.FirstOrDefaultAsync();
        }

        public async Task sayHi()
        {
            Console.WriteLine("hiii");
        }
    }

    public class StoreMessageResult
    {
        public bool isSuccess { get; set; }
        public DateTime CreateDateTime { get; set; }
        public MessageModel Message { get; set; }

        public StoreMessageResult(bool isSuccess, DateTime dateTime, MessageModel message)
        {
            this.isSuccess = isSuccess;
            CreateDateTime = dateTime;
            Message = message;
        }
    }
}