using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moka.Server.Auth;
using Moka.Server.Data;
using Moka.Server.Events;
using Moka.Server.Helper;
using Moka.Server.Models;

namespace Moka.Server.Service
{
    public class MokaUserService : MokaUser.MokaUserBase
    {
        private readonly ILogger<MokaUserService> _logger;
        private UserEvents _userEvents;
        private readonly IUserService _userService;
        private IJWTAuthenticationManager _jwtAuthentication;

        public MokaUserService(ILogger<MokaUserService> logger, UserEvents userEvents, IUserService userService,
            IJWTAuthenticationManager jwtAuthentication)
        {
            _logger = logger;
            _userEvents = userEvents;
            _userService = userService;
            _jwtAuthentication = jwtAuthentication;
        }

        public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            var user = UserModel.FromUser(request.User);
            user.Password = request.Password;
            user.Devices.Add(new Device("REGISTER", request.MacAddress, "1234", "1234", DateTime.Now, DateTime.Now));
            var result = await _userService.FindOrCreate(user);
            return new RegisterResponse
            {
                IsSuccess = result.IsCreated
            };
        }

        public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            var user = await _userService.FindAsync(request.Username);
            if (user != null && request.Password.Equals(user.Password))
            {
                var oldDevice = user.Devices.FirstOrDefault(x => x.MacAddress == request.MacAddress);
                var salt = SecurityHelper.RandomString();
                var totp = SecurityHelper.RandomString(256);
                var token = _jwtAuthentication.GenerateToken(user.Id,request.MacAddress);
                if (oldDevice == null)
                {
                    user.Devices.Add(new Device(request.MacAddress,
                        request.MacAddress,
                        totp,
                        salt,
                        DateTime.Now,
                        DateTime.Now
                    ));
                }
                else
                {
                    foreach (var device in user.Devices.Where(x => x.MacAddress == request.MacAddress))
                    {
                        device.Salt = salt;
                        device.Totp = totp;
                        device.LastConnect = DateTime.Now;
                    }
                }

                await _userService.Update(user);
                var userModel = user.ToUserModel();
                var protoUser = user.ToUserModel().ToUser();

                return new LoginResponse
                {
                    IsSuccess = true,
                    Salt = salt,
                    Token = token,
                    Totp = totp,
                    User = protoUser
                };
            }

            return new LoginResponse
            {
                IsSuccess = false
            };
        }
    }
}