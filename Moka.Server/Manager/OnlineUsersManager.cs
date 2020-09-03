using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moka.Server.Events;
using Moka.Server.Models;

namespace Moka.Server.Manager
{
    // public interface IOnlineUsersManager
    // {
    //     void Join(string name, IServerStreamWriter<RecievedMessageData> response);
    //     void Remove(string name);
    //     Task SendMeesageIfOnline(string user, RecievedMessageData data);
    // }

    public class OnlineUsersManager
    {
        private readonly ILogger<OnlineUsersManager> _logger;

        private ConcurrentDictionary<Guid, IServerStreamWriter<Message>> users;
        private MessageEvents _messageEvents;
        private UserEvents _userEvents;

        public OnlineUsersManager(ILogger<OnlineUsersManager> logger, MessageEvents messageEvents,
            UserEvents userEvents)
        {
            _logger = logger;
            _messageEvents = messageEvents;
            _userEvents = userEvents;
            users = new ConcurrentDictionary<Guid, IServerStreamWriter<Message>>();
            _messageEvents.MessageReceived += OnMessageReceived;
            _userEvents.UserOnlined += OnUserOnlined;
            _userEvents.UserOfflined += OnUserOfflined;
        }

        public void Join(Guid guid, IServerStreamWriter<Message> response)
        {
            _logger.LogInformation("User added to onlines " + guid);
            users.TryAdd(guid, response);
            _logger.LogInformation("onlines" + users.Count);
        }

        protected virtual void OnMessageReceived(object sender, Message message)
        {
            SendMeesageIfOnline(message);
        }

        protected virtual void OnUserOnlined(object sender, UserOnlineEventArgs eventArgs)
        {
            _logger.LogInformation("user became Online EVENT" + eventArgs.User.Guid);
            Join(eventArgs.User.Guid, eventArgs.StreamWriter);
        }

        protected virtual void OnUserOfflined(object sender, UserModel userModel)
        {
            _logger.LogInformation("user became Offline EVENT" + userModel.Guid);
            Remove(userModel.Guid);
        }

        public void Remove(Guid guid) => users.TryRemove(guid, out var s);

        public async Task SendMeesageIfOnline(Message message)
        {
            _logger.LogInformation("onlines sending" + users.Count);
            var to = Guid.Parse(message.ReceiverId);
            if (users.ContainsKey(to))
            {
                await users[to].WriteAsync(message);
            }
            else
            {
                _logger.LogInformation("user offline" + to);
            }
        }
    }
}