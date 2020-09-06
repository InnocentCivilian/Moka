using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moka.Server.Service;
using Moka.SharedKernel.Security;

namespace Moka.Server.Auth
{
    public class BasicAuthenticationOptions : AuthenticationSchemeOptions
    {
    }

    public class CustomAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        private readonly ICustomAuthenticationManager customAuthenticationManager;
        private readonly IUserService _userService;

        public CustomAuthenticationHandler(
            IOptionsMonitor<BasicAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            ICustomAuthenticationManager customAuthenticationManager, IUserService userService)
            : base(options, logger, encoder, clock)
        {
            this.customAuthenticationManager = customAuthenticationManager;
            _userService = userService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Unauthorized 1");

            string authorizationHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return AuthenticateResult.NoResult();
            }

            if (!authorizationHeader.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.Fail("Unauthorized 2");
            }

            string token = authorizationHeader.Substring("Bearer".Length).Trim();
            string totp = Request.Headers["Totp"];

            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.Fail("Unauthorized 3");
            }

            try
            {
                return validateToken(token,totp);
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex.Message);
            }
        }

        private AuthenticateResult validateToken(string token,string totp)
        {
            base.Logger.LogInformation("token:" + token);
            var result = customAuthenticationManager.ValidateToken(token);
            if (!result.IsSuccess)
            {
                return AuthenticateResult.Fail("Unauthorized 4");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.Id),
                new Claim(ClaimTypes.System, result.Mac),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new GenericPrincipal(identity, new[] {"user"});
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            var user = _userService.FindById(result.Id);
            var device = user.Devices.FirstOrDefault(x => x.MacAddress == result.Mac);
            if (device==null)
            {
                return AuthenticateResult.Fail("Unauthorized 5");
            }

            if (TotpHelper.Validate(TotpHelper.CalculateSecret(device.MacAddress,device.Salt,user.Guid.ToString()),totp,device.Salt))
            {
                return AuthenticateResult.Success(ticket);
            }
            return AuthenticateResult.Fail("Unauthorized 6");
        }
    }
}