using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moka.Server.Data;

namespace Moka.Server.Auth
{
    public interface ICustomAuthenticationManager
    {
        JwtValidationResult ValidateToken(string token);
    }

    public class JwtValidationResult
    {
        public bool IsSuccess { get; set; }
        public string Id { get; set; }
        public string Mac { get; set; }
    }
    public class CustomAuthenticationManager : ICustomAuthenticationManager
    {
        private readonly string tokenKey;
        private readonly string myIssuer;
        private readonly string myAudience;

        public CustomAuthenticationManager(string tokenKey, string myIssuer, string myAudience )
        {
            this.tokenKey = tokenKey;
            this.myIssuer = myIssuer;
            this.myAudience = myAudience;
        }

        public JwtValidationResult ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenKey));

            try
            {
                var u =tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = myIssuer,
                    ValidAudience = myAudience,
                    IssuerSigningKey = mySecurityKey
                }, out SecurityToken validatedToken);
                Console.WriteLine("found identity"+validatedToken.Id);
                Console.WriteLine("found identity"+u.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
                // return u.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                return new JwtValidationResult
                {
                    IsSuccess = true,
                    Id = u.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value,
                    Mac = u.Claims.First(x => x.Type == ClaimTypes.System).Value
                };
            }
            catch
            {
                return new JwtValidationResult
                {
                    IsSuccess = false
                };
            }
        }
    }
}