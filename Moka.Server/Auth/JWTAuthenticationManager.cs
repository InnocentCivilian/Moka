using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Moka.Server.Auth
{
    public interface IJWTAuthenticationManager
    {
        string GenerateToken(string name);
    }

    public class JWTAuthenticationManager : IJWTAuthenticationManager
    {
        private readonly string tokenKey;
        private readonly string myIssuer;
        private readonly string myAudience;
        

        public JWTAuthenticationManager(string tokenKey, string myIssuer, string myAudience)
        {
            this.tokenKey = tokenKey;
            this.myIssuer = myIssuer;
            this.myAudience = myAudience;
        }

        public string GenerateToken(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new InvalidOperationException("Id is not specified.");
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            // var tokenKey = Configuration.GetValue<string>("TokenKey");
            var key = Encoding.ASCII.GetBytes(tokenKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, id)
                }),
                Expires = DateTime.UtcNow.AddHours(10),
                Issuer = myIssuer,
                Audience = myAudience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
            // var claims = new[] { new Claim(ClaimTypes.Name, name) };
            // var credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
            // var token = new JwtSecurityToken("ExampleServer", "ExampleClients", claims, expires: DateTime.Now.AddDays(60), signingCredentials: credentials);
            // return JwtTokenHandler.WriteToken(token);
        }
    }
}