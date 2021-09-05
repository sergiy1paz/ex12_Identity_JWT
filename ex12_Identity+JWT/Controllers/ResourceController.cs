using ex12_Identity_JWT.Database;
using ex12_Identity_JWT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ex12_Identity_JWT.Controllers
{
    /*
     * При запуску в класі Program організована ініціалізація бази даниз початковими записами
     * В тому ж місці створюється один адміністратор 
     * #Admin
     *      "username": "sergo_admin123",
     *      "password": "Admin_123",
     * 
     * Інших адміністраторів або модераторів може створити лише адміністратор, тобто для цього потрібно авторизуватися під #Admin
     * #Admin має доступ до всіх методів цього контроллера
     * 
     * 
     * Examples:
     *      Register:
     *      #1 Має доступ до майже до всіх методів окрім GetDeveloperPage() та CheckModerator() так як не має поля Company і не є модератором
     *        "username": "Sophia",
     *        "email": "Sophia@gmail.com",
     *        "password": "Sophia_2000",
     *        "repeatpassword": "Sophia_2000",
     *        "Age": 21,
     *        "Subscription": "Premium"
     *        
     *      #2  Має доступ лише до Index() /home/index
     *          "username": "Rima",
     *          "email": "Rima@gmail.com",
     *          "password": "Rima_2005",
     *          "repeatpassword": "Rima_2005",
     *          "Age": 21,
     * **/


    [Route("home")]
    [ApiController]
    public class ResourceController : ControllerBase
    {


        // home/index
        [HttpGet("[action]")]
        [Authorize]
        public IActionResult Index()
        {
            var name = HttpContext.User.Identity.Name;
            return Ok($"You have been authorized! Hello {name}");
        }

        //home/CheckModerator
        [HttpGet("[action]")]
        [Authorize(Roles = Roles.MODERATOR)]
        public IActionResult CheckModerator()
        {
            return Ok("You are moderator!");
        }

        // /home/users
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetUsers([FromServices]UserManager<User> userManager)
        {
            var usersInfo = userManager.Users
                .Select(user => new 
                { 
                    UserName = user.UserName,
                    Email = user.Email,
                    Subscription = user.Subscription
                }).ToList();

            return Ok(usersInfo);
        }

        // /home/subscriptions
        [HttpGet("subscriptions")]
        [Authorize(Policy = "AgeLimit")]
        public IActionResult GetSubscriptions([FromServices]ApplicationContext db)
        {
            return Ok(db.Subscriptions.ToList());
        }

        // /home/content
        [HttpGet("content")]
        [Authorize(Policy = "PaidContent")]
        public IActionResult GetContent()
        {
            return Ok("Some paid content...");
        }

        // /home/developer
        [HttpGet("developer")]
        [Authorize(Policy = "Developer")]
        public IActionResult GetDeveloperPage()
        {
            return Ok("Developer page....");
        }
    }
}
