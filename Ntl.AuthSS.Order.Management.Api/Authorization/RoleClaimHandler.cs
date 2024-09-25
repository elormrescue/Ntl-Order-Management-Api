using iText.Kernel.Colors;
using Microsoft.AspNetCore.Authorization;
using Ntl.AuthSS.Order_Management.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ntl.AuthSS.Order_Management.Api.Authorization
{
    public class RoleClaimHandler : AuthorizationHandler<DeliveryAppRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   DeliveryAppRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "Role"))
                return Task.CompletedTask;

            var role = context.User.FindFirst(x => x.Type == "Role").Value;

            if (role == Roles.ShipperAgent.ToString() || role == Roles.ShipperAdmin.ToString())
                context.Succeed(requirement);

            return Task.CompletedTask;

        }
    }
}
