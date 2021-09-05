using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ex12_Identity_JWT.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Subscription will have due date in Future
        // public DateTime Expires { get; set; }

        public List<User> Users { get; set; }
    }
}
