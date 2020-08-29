using System.Collections.Concurrent;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

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

        private ConcurrentDictionary<string, IServerStreamWriter<RecievedMessageData>> users;

        public OnlineUsersManager(ILogger<OnlineUsersManager> logger)
        {
            _logger = logger;
            users = new ConcurrentDictionary<string, IServerStreamWriter<RecievedMessageData>>();
        }

        public void Join(string name, IServerStreamWriter<RecievedMessageData> response)
        {
            _logger.LogInformation("User added to onlines" + name);
            users.TryAdd(name, response);
            _logger.LogInformation("onlines"+users.Count);
        }

        public void Remove(string name) => users.TryRemove(name, out var s);

        public async Task SendMeesageIfOnline(string user, RecievedMessageData data)
        {
            _logger.LogInformation("onlines sending"+users.Count);

            if (users.ContainsKey(user))
            {
                await users[user].WriteAsync(data);
            }
            else
            {
                _logger.LogInformation("user offline" + user);
            }
        }
    }
}