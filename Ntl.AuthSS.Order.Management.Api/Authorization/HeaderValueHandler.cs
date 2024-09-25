using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ntl.Tss.Identity.Data;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ntl.AuthSS.Order_Management.Api.Authorization
{
    public class HeaderValueHandler : AuthorizationHandler<DeliveryAppRequirement>
    {
        private readonly IServiceProvider _serviceProvider;
        public HeaderValueHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   DeliveryAppRequirement requirement)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var httpContext = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                var tssDbContext = scope.ServiceProvider.GetRequiredService<TssIdentityDbContext>();
                var deviceIdHeader = httpContext.Request.Headers.ContainsKey("DeviceId");
                if (!deviceIdHeader)
                    context.Fail();

                var userIdClaim = context.User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    context.Fail();

                var deviceId = httpContext.Request.Headers["DeviceId"].ToString();
                var userId = Convert.ToInt32(userIdClaim.Value);

                var userDevice = tssDbContext.UserDevices.SingleOrDefault(x => x.IsActive && x.User.Id == userId);
                if (userDevice != null && userDevice.DeviceId == deviceId)
                    context.Succeed(requirement);
                else
                    context.Fail();

                return Task.CompletedTask;
            }
               
        }        
    }
}
