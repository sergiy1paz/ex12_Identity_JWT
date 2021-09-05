using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ex12_Identity_JWT.DTO
{
    public class OutputUserModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
