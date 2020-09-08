using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moka.Server.Data;
using Moka.Server.Events;
using Moka.Server.Manager;

namespace Moka.Server.Service
{
    // [Authorize(Policy = "protectedScope")]
    [Authorize]

    public class MokaMessageService : MokaMessenger.MokaMessengerBase
    {
        private readonly ILogger<MokaMessageService> _logger;
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;
        private readonly OnlineUsersManager _onlineUsers;
        private MessageEvents _messageEvents;
        private UserEvents _userEvents;
        private UserData _currentUser;
        private IHttpContextAccessor _httpContextAccessor;

        public MokaMessageService(ILogger<MokaMessageService> logger, IUserService userService,
            OnlineUsersManager onlineUsers, IMessageService messageService, MessageEvents messageEvents,
            UserEvents userEvents, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _userService = userService;
            _onlineUsers = onlineUsers;
            _messageService = messageService;
            _messageEvents = messageEvents;
            _userEvents = userEvents;
            _httpContextAccessor = httpContextAccessor;
            var user = httpContextAccessor.HttpContext.User;
            if (user.Identity.IsAuthenticated)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier).Value;
                _currentUser = _userService.FindById(userId);
            }
            
        }

        // public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        // {
        //     _logger.LogInformation("registeing user {Name}", request.User.Nickname);
        //     var res = await _userService.FindOrCreate(new UserModel(Guid.Empty, request.User.Nickname, request.Password,
        //         DateTime.Now));
        //     return await Task.FromResult(new RegisterResponse
        //     {
        //         Id = res.UserModel.Guid.ToString()
        //     });
        // }

        public override async Task GetMessageStream(Empty _, IServerStreamWriter<Message> responseStream,
            ServerCallContext context)
        {
            _logger.LogInformation("message stream user call: {Name}", _currentUser.NickName);
            
            _userEvents.OnUserOnlined(
                new UserOnlineEventArgs {StreamWriter = responseStream, User = _currentUser.ToUserModel()});
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100);
            }

            _userEvents.OnUserOfflined(_currentUser.ToUserModel());
        }

        public override async Task<Message> SendMessage(Message request,
            ServerCallContext context)
        {

            var reciever = await _userService.FindAsync(Guid.Parse(request.ReceiverId));
            request.SenderId = _currentUser.Guid.ToString();
            request.ReceiverId = reciever.Guid.ToString();
            var stored = await _messageService.Store(request);
            _logger.LogInformation("new message stored {id}",stored.Id);
            _messageEvents.OnMessageReceived(stored.ToMessage());
            return stored.ToMessage();
        }

        public override async Task<MessageArray> GetOfflineMessageStream(Empty request, ServerCallContext context)
        {
            var messagesData = await _messageService.FindUserUndeliverdInboxMessages(_currentUser);
            var messages = messagesData.Select(x => x.ToMessage()).ToList();
            return await Task.FromResult(
                new MessageArray
                {
                    Messages = {messages}
                }
            );
        }

        public override async Task<Empty> CumulativeAck(MessageAck request, ServerCallContext context)
        {
            await _messageService.UpdateAck(_currentUser.ToUserModel(), request);
            return new Empty();
        }
    }
}