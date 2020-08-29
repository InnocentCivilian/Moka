using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using UserModel = Moka.Server.Models.UserModel;

namespace Moka.Server.Service
{
    // [Authorize(Policy = "protectedScope")]
    public class MokaMessageService : MokaMessenger.MokaMessengerBase
    {
        private readonly ILogger<MokaMessageService> _logger;
        private readonly UserService _userService;

        public MokaMessageService(ILogger<MokaMessageService> logger, UserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            _logger.LogInformation("registeing user {Name}", request.User.Nickname);
            var res = await _userService.FindOrCreate(new UserModel(Guid.Empty, request.User.Nickname, request.Password,
                DateTime.Now));
            return await Task.FromResult(new RegisterResponse
            {
                Id = res.UserModel.Guid.ToString()
            });
        }

        public override async Task GetWeatherStream(Empty _, IServerStreamWriter<WeatherData> responseStream,
            ServerCallContext context)
        {
            _logger.LogInformation("user call: {Name}", context.RequestHeaders.Get("User"));
            
            var rng = new Random();
            var now = DateTime.UtcNow;
            var Summaries = new string[] {"sunny", "cloudy", "rainy"};
            var i = 0;
            while (!context.CancellationToken.IsCancellationRequested && i < 20)
            {
                await Task.Delay(500); // Gotta look busy

                var forecast = new WeatherData
                {
                    DateTimeStamp = Timestamp.FromDateTime(now.AddDays(i++)),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                };

                _logger.LogInformation("Sending WeatherData response");

                await responseStream.WriteAsync(forecast);
            }
        }
    }
}