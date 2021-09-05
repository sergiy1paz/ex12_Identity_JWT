using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ex12_Identity_JWT.Models;
using Microsoft.IdentityModel.Tokens;


namespace ex12_Identity_JWT.JWT.Interfaces
{
    public interface IJWTGenerator
    {
        string CreateToken(User user, IList<string> roles, IList<Claim> claims);

        public static SymmetricSecurityKey GetSymmetricSecurityKey(string key)
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
        }
    }
}
