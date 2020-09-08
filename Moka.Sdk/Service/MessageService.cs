using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moka.Sdk.SqlLite;

namespace Moka.Sdk.Service
{
    public interface IMessageService
    {
        Task Store(MessageLite message);
        Task StoreMany(IEnumerable<MessageLite> messages);
    }

    public class MessageService : IMessageService
    {
        private MokaClientContext _Context { get; set; }
        public ILogger<MessageService> _logger { get; set; }

        public MessageService(MokaClientContext context, ILogger<MessageService> logger)
        {
            _Context = context;
            _logger = logger;
        }

        public async Task Store(MessageLite message)
        {
            _logger.LogDebug("storing message");
            await _Context.Messages.AddAsync(message);
            await _Context.SaveChangesAsync();

        }
        public async Task StoreMany(IEnumerable<MessageLite> messages)
        {
            _logger.LogDebug($"storing bunch of messages count {messages.Count()}");
            await _Context.Messages.AddRangeAsync(messages);
            await _Context.SaveChangesAsync();

        }
    }
}