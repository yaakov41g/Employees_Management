using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EmployeeManagement.Security  // this code does not fullfill the requirement correctly. some times 'context.Resource' is null. so , I replaced it by cod from 
{   //- https://www.youtube.com/redirect?q=https%3A%2F%2Fstackoverflow.com%2Fquestions%2F59197631%2Fcontext-resource-as-authorizationfiltercontext-returning-null-in-asp-net-core&redir_token=QUFFLUhqbWc0MF9iVWU2TUZ3cFpCeDlLS3NSYTk5TGFOZ3xBQ3Jtc0trdHhGR3haUnVEYjlkaWtEWFJIakRENkxXb1U0czBhMVpveXhZSlBBUDdSSTJSazg3NkFBWHVza3VpYzVTTUlISi1TMkZIbHp3d1c2YUF6X2NvQkNjNEJJSWZYSGxVSkYxOWN6bG9sMkhDaFprMk50TQ%3D%3D&event=comments&stzid=UgwRSbNBCUDO16JKrD14AaABAg.96-psLH0d4H9IBa8lIil5H
    /*public class CanEditOnlyOtherAdminRolesAndClaimsHandler :
      AuthorizationHandler<ManageAdminRolesAndClaimsRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
           ManageAdminRolesAndClaimsRequirement requirement)
        {
            var authFilterContext = context.Resource as AuthorizationFilterContext;
            if (authFilterContext == null)
            {
                return Task.CompletedTask;
            }

            string loggedInAdminId =
                context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            string adminIdBeingEdited = authFilterContext.HttpContext.Request.Query["userId"];

            if (context.User.IsInRole("Administrator") &&
                context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") &&
                adminIdBeingEdited.ToLower() != loggedInAdminId.ToLower())
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }*/
    //  The next code is from https://www.youtube.com/redirect?q=https%3A%2F%2Fstackoverflow.com%2Fquestions%2F59197631%2Fcontext-resource-as-authorizationfiltercontext-returning-null-in-asp-net-core&redir_token=QUFFLUhqbWc0MF9iVWU2TUZ3cFpCeDlLS3NSYTk5TGFOZ3xBQ3Jtc0trdHhGR3haUnVEYjlkaWtEWFJIakRENkxXb1U0czBhMVpveXhZSlBBUDdSSTJSazg3NkFBWHVza3VpYzVTTUlISi1TMkZIbHp3d1c2YUF6X2NvQkNjNEJJSWZYSGxVSkYxOWN6bG9sMkhDaFprMk50TQ%3D%3D&event=comments&stzid=UgwRSbNBCUDO16JKrD14AaABAg.96-psLH0d4H9IBa8lIil5H
    //- see there answer from "user12838074"
    public class CanEditOnlyOtherAdminRolesAndClaimsHandler :
                     AuthorizationHandler<ManageAdminRolesAndClaimsRequirement>
         {
             private readonly IHttpContextAccessor httpContextAccessor;
             public CanEditOnlyOtherAdminRolesAndClaimsHandler(
                     IHttpContextAccessor httpContextAccessor)
             {
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                ManageAdminRolesAndClaimsRequirement requirement)
        {

            var loggedInAdminId = context.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value.ToString();

            var adminIdBeingEdited = httpContextAccessor.HttpContext
                .Request.Query["userId"].ToString();

            if (context.User.IsInRole("Administrator")
                 && context.User.HasClaim(c => c.Type == "Edit Role" && c.Value == "true")
                 && adminIdBeingEdited.ToLower() != loggedInAdminId.ToLower())
            {
                context.Succeed(requirement);
            }
            else
            { context.Fail(); } // we should not use this because this couse others handlers to fail even if the user fulfills their conditions. see more in Security/handlersRules.txt in the Solution Explorer list.

            return Task.CompletedTask;

        }

    }
}
