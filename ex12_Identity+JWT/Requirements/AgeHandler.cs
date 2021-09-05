using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ex12_Identity_JWT.Requirements
{
    public class AgeHandler : AuthorizationHandler<AgeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AgeRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == ClaimTypes.DateOfBirth)) 
            {
                if (int.TryParse(context.User.FindFirst(claim => claim.Type == ClaimTypes.DateOfBirth).Value, out int yearsOld))
                {
                    if (yearsOld >= requirement.Age)
                    {
                        context.Succeed(requirement);
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
