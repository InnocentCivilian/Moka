using Microsoft.Extensions.Logging;
using Moka.Sdk.SqlLite;

namespace Moka.Sdk.Service
{
    public interface IUserService
    {
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
    }
}