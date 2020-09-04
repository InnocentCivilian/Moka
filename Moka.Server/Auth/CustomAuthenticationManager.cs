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
        string? ValidateToken(string token);
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

        public string ValidateToken(string token)
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
                return u.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            }
            catch
            {
                return null;
            }
        }
    }
}