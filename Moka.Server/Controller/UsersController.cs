using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moka.Server.Models;
using Moka.Server.Service;

namespace Moka.Server.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController :ControllerBase
    {
        private readonly UserService _userService;
        private readonly MessageService _messageService;

        public UsersController(UserService userService, MessageService messageService)
        {
            _userService = userService;
            _messageService = messageService;
        }
        [HttpGet]
        public async Task<ActionResult<User>>index()
        {
            var userHooman = new User(Guid.Empty,"hooman","1234",DateTime.Now);
            var hoomanCreated = await _userService.FindOrCreate(userHooman);
            var userFaezeh = new User(Guid.Empty,"faezeh","1234",DateTime.Now);
            var faezehCreated= await _userService.FindOrCreate(userFaezeh);
            var msg = new Message(Guid.Empty, Encoding.UTF8.GetBytes("hi love"),MessageType.Text,hoomanCreated.User,faezehCreated.User,DateTime.Now);
            return Ok(await _messageService.Store(msg));
        }
    }
}