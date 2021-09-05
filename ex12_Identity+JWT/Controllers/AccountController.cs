using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ex12_Identity_JWT.Models;
using ex12_Identity_JWT.DTO;
using ex12_Identity_JWT.JWT.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ex12_Identity_JWT.Requirements.Claims;
using ex12_Identity_JWT.Database;

namespace ex12_Identity_JWT.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJWTGenerator _jWTGenerator;
        public AccountController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
            IJWTGenerator jWTGenerator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jWTGenerator = jWTGenerator;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Register(RegisterModel registerModel, 
            [FromServices]ApplicationContext db)
        {
            if (ModelState.IsValid)
            {
                if (registerModel is not null)
                {
                    var user = new User
                    {
                        Email = registerModel.Email,
                        UserName = registerModel.Username,
                        Age = registerModel.Age.Value,
                        Company = registerModel.Company,
                        Subscription = GetSubscription(registerModel.Subscription, db)
                        // add more
                    };

                    var result = await _userManager.CreateAsync(user, registerModel.Password);
                    // here i can add to role and add claims
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddToRoleAsync(user, Roles.USER);
                        if (result.Succeeded)
                        {
                            result = await _userManager.AddClaimsAsync(user, CreateClaims(registerModel));
                        }
                    }

                    if (result.Succeeded)
                    {
                        var claims = await _userManager.GetClaimsAsync(user);
                        var roles = await _userManager.GetRolesAsync(user);
                        return Ok(new OutputUserModel
                        {
                            Username = user.UserName,
                            Token = _jWTGenerator.CreateToken(user, roles, claims),
                            Email = user.Email
                        });
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("Register", error.Description);
                        }
                    }

                }
            }
            return BadRequest(ModelState);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Login(LoginModel loginModel,
            [FromServices]SignInManager<User> signInManager)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(loginModel.UserName);
                if (user is not null)
                {
                    var result = await signInManager.CheckPasswordSignInAsync(user, loginModel.Password, false);

                    if (result.Succeeded)
                    {
                        var claims = await _userManager.GetClaimsAsync(user);
                        var roles = await _userManager.GetRolesAsync(user);
                        return new ObjectResult(new OutputUserModel
                        {
                            Username = user.UserName,
                            Email = user.Email,
                            Token = _jWTGenerator.CreateToken(user, roles, claims)
                        });
                    } else
                    {
                        return BadRequest("Password is not valid!");
                    }
                } else
                {
                    return BadRequest("Such user does not exist!");
                }
            }
            return Unauthorized(ModelState);
        }


        /*
         * Example:
         *      Register moderator:
         *          "username": "Vadim",
         *          "email": "Vadim@gmail.com",
         *          "password": "Vadim_2003",
         *          "repeatpassword": "Vadim_2003",
         *          "Age": 18,
         *          "Subscription": "Premium",
         *          "Company": "SomeCompany"
         *          
         *          
         **/
        [HttpPost("create")]
        [Authorize(Roles = Roles.ADMIN)]
        // here admin can create another admins or moderator
        // he also can create users
        public async Task<IActionResult> CreateAccount(RegisterModel registerModel,
            [FromServices]ApplicationContext db, [FromQuery]string role)
        {
            if (ModelState.IsValid)
            {
                if (registerModel is not null)
                {
                    var user = new User
                    {
                        Email = registerModel.Email,
                        UserName = registerModel.Username,
                        Age = registerModel.Age.Value,
                        Company = registerModel.Company,
                        Subscription = GetSubscription(registerModel.Subscription, db)
                        // add more
                    };

                    

                    var result = await _userManager.CreateAsync(user, registerModel.Password);
                    // here i can add to role and add claims
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddToRoleAsync(user, GetRole(role));
                        if (result.Succeeded)
                        {
                            result = await _userManager.AddClaimsAsync(user, CreateClaims(registerModel));
                        }
                    }

                    if (result.Succeeded)
                    {
                        var claims = await _userManager.GetClaimsAsync(user);
                        var roles = await _userManager.GetRolesAsync(user);
                        return Ok(new OutputUserModel
                        {
                            Username = user.UserName,
                            Token = _jWTGenerator.CreateToken(user, roles, claims),
                            Email = user.Email
                        });
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("Register", error.Description);
                        }
                    }

                }
            }
            return BadRequest(ModelState);
        }

        private List<Claim> CreateClaims(RegisterModel registerModel)
        {
            var claims = new List<Claim>();
            if (registerModel.Age is not null)
            {
                claims.Add(new Claim(ClaimTypes.DateOfBirth, registerModel.Age.ToString()));
            }
            if (registerModel.Company is not null)
            {
                claims.Add(new Claim(CustomClaims.COMPANY, registerModel.Company));
            }
            if (registerModel.Subscription is not null)
            {
                claims.Add(new Claim(CustomClaims.SUBSCRIPTION, registerModel.Subscription));
            }
            if (registerModel.Email is not null)
            {
                claims.Add(new Claim(ClaimTypes.Email, registerModel.Email));
            }

            return claims;
        }
        private string GetRole(string role) => role.ToLower() switch
        {
            "admin" => Roles.ADMIN,
            "moderator" => Roles.MODERATOR,
            _ => Roles.USER
        };
        private Subscription GetSubscription(string subscriptionName, ApplicationContext db)
        {
            if (subscriptionName is null)
            {
                return db.Subscriptions.FirstOrDefault(subsc => subsc.Name == Subscriptions.FREE);
            }

            switch (subscriptionName.ToLower())
            {
                case "vip":
                    return db.Subscriptions.FirstOrDefault(subsc => subsc.Name == Subscriptions.VIP);
                case "premium":
                    return db.Subscriptions.FirstOrDefault(subsc => subsc.Name == Subscriptions.PREMIUM);
                default:
                    return db.Subscriptions.FirstOrDefault(subsc => subsc.Name == Subscriptions.FREE);
            }
        }
            
    }
}
