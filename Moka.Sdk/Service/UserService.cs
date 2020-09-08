using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Microsoft.Extensions.Logging;
using Moka.Sdk.SqlLite;

namespace Moka.Sdk.Service
{
    public interface IUserService
    {
        public List<Guid> NewUsers(List<Guid> users);
        public Task StoreAsync(UserLite user);
    }

    public class UserService : IUserService
    {
        private MokaClientContext _Context { get; set; }
        public ILogger<UserService> _logger { get; set; }

        public UserService(MokaClientContext context, ILogger<UserService> logger)
        {
            _Context = context;
            _logger = logger;
        }

        public List<Guid> NewUsers(List<Guid> users)
        {
            var notExist = users.Where(
                x => !_Context.Users.Any(u => u.Guid == x)
            ).ToList();
            return notExist;
        }

        public async Task StoreAsync(UserLite user)
        {
            _logger.LogDebug($"storing user {user.ToString()}");
            await _Context.Users.AddAsync(user);
            await _Context.SaveChangesAsync();
        }
    }
}