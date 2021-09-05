using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ex12_Identity_JWT.Models;
using ex12_Identity_JWT.JWT.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;


namespace ex12_Identity_JWT.JWT.Implementations
{
    public class JWTGenerator : IJWTGenerator
    {
        private readonly SymmetricSecurityKey _key;
        private readonly IConfiguration _config;

        public JWTGenerator(IConfiguration config)
        {
            _config = config;
            _key = IJWTGenerator.GetSymmetricSecurityKey(_config["AuthOptions:TokenKey"]);
        }
        
        public string CreateToken(User user, IList<string> roles, IList<Claim> claimsList)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, role));
            }

            claims.AddRange(claimsList);

            var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer: _config["AuthOptions:Issuer"],
                audience: _config["AuthOptions:Audience"],
                notBefore: now,
                claims: claims,
                expires: now.AddMinutes(int.Parse(_config["AuthOptions:Lifetime"])),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
