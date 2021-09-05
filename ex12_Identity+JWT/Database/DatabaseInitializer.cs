using ex12_Identity_JWT.Models;
using ex12_Identity_JWT.Requirements.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace ex12_Identity_JWT.Database
{
    public class DatabaseInitializer
    {
        public static async Task InitializeAsync(IConfiguration config, UserManager<User> userManager, 
            RoleManager<IdentityRole> roleManager, ApplicationContext db)
        {
            if (!roleManager.Roles.Any(role => role.Name == Roles.ADMIN))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.ADMIN));
            } 
            if (!roleManager.Roles.Any(role => role.Name == Roles.MODERATOR))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.MODERATOR));
            }
            if (!roleManager.Roles.Any(role => role.Name == Roles.USER))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.USER));
            }

            if (await userManager.FindByNameAsync(config["Admin:Username"]) is null)
            {
                var admin = new User 
                { 
                    UserName = config["Admin:Username"], 
                    Age = 19, 
                    Company = "Existek",
                    Subscription = db.Subscriptions.FirstOrDefault(subsc => subsc.Name == Subscriptions.VIP)
                };

                var result = await userManager.CreateAsync(admin, config["Admin:Password"]);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, Roles.ADMIN);
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.DateOfBirth, admin.Age.ToString()),
                        new Claim(CustomClaims.COMPANY, admin.Company),
                        new Claim(CustomClaims.SUBSCRIPTION, admin.Subscription.Name)
                    };
                    await userManager.AddClaimsAsync(admin, claims);
                }
            }
        }

        public static async Task InitializeSubscriptionsAsync(ApplicationContext db)
        {
            if (!db.Subscriptions.Any(s => s.Name == Subscriptions.FREE))
            {
                await db.Subscriptions.AddAsync(new Subscription { Name = Subscriptions.FREE });
            }
            if (!db.Subscriptions.Any(s => s.Name == Subscriptions.PREMIUM))
            {
                await db.Subscriptions.AddAsync(new Subscription { Name = Subscriptions.PREMIUM });
            }
            if (!db.Subscriptions.Any(s => s.Name == Subscriptions.VIP))
            {
                await db.Subscriptions.AddAsync(new Subscription { Name = Subscriptions.VIP });
            }
        }
    }
}
