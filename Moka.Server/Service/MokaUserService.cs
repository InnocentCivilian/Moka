using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moka.Server.Data;
using Moka.Server.Events;
using Moka.Server.Models;

namespace Moka.Server.Service
{
    public class MokaUserService : MokaUser.MokaUserBase
    {
        private readonly ILogger<MokaUserService> _logger;
        private UserEvents _userEvents;
        private readonly IUserService _userService;

        public MokaUserService(ILogger<MokaUserService> logger, UserEvents userEvents, IUserService userService)
        {
            _logger = logger;
            _userEvents = userEvents;
            _userService = userService;
        }

        public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            var user = UserModel.FromUser(request.User);
            user.Password = request.Password;
            user.Devices.Add(new Device("REGISTER",request.MacAddress,"1234","1234",DateTime.Now, DateTime.Now));
            var result = await _userService.FindOrCreate(user);
            return new RegisterResponse
            {
                IsSuccess = result.IsCreated
            };
        }
    }
}