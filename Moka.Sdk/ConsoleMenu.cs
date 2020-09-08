using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Moka.Sdk
{
    public class ConsoleMenu
    {
        private IMe me { get; set; }
        private ILogger<ConsoleMenu> _logger;

        public ConsoleMenu(IMe me, ILogger<ConsoleMenu> logger)
        {
            this.me = me;
            _logger = logger;
            _logger.LogDebug("Moke Client Console Menu Running...");

        }

        public async Task ShowMenu()
        {
            _logger.LogDebug("Username :  "+ me.User.Username);

            _logger.LogDebug("gRPC MOKA CLI Client");
            _logger.LogDebug("");
            _logger.LogDebug("Press a key:");
            _logger.LogDebug("1: Register");
            _logger.LogDebug("2: Login");
            _logger.LogDebug("3: Login(FAIL)");
            _logger.LogDebug("5: TOTP");
            _logger.LogDebug("6: Send Message To opposit");
            _logger.LogDebug("0: Exit");
            _logger.LogDebug("");
            var exiting = false;
            while (!exiting)
            {
                var consoleKeyInfo = Console.ReadKey(intercept: true);
                switch (consoleKeyInfo.KeyChar)
                {
                    case '1':
                        var registerResp = await me.Register();
                        _logger.LogDebug("Register:" + registerResp);
                        break;
                    case '2':
                        me.Password = EnvConsts.PASSWORD;
                        var loginResponse = await me.Login();
                        _logger.LogDebug("Login:" + loginResponse);
                        break;
                    case '3':
                        me.Password = EnvConsts.WRONGPASSWORD;
                        var failLoginResponse = await me.Login();
                        _logger.LogDebug("Login:" + failLoginResponse);
                        break;
                    case '5':
                        var totp = me.CalculateTotp();
                        _logger.LogDebug("Totp:" + totp);
                        break;
                    case '6':
                        var opstmsg = await me.SendMessageToOpposit();
                        _logger.LogDebug("message sent to "+ opstmsg.ReceiverId+" msg id ="+opstmsg.Id);
                        break;
                    case '0':
                        exiting = true;
                        break;
                }
            }

            _logger.LogDebug("Exiting");
        }
    }
}