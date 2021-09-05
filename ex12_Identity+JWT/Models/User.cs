using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;


namespace ex12_Identity_JWT.Models
{
    public class User : IdentityUser
    {
        // add some prop
        public int Age { get; set; }
        public string Company { get; set; }

        public int? SubscriptionId { get; set; }
        public Subscription Subscription { get; set; }
        
    }
}
