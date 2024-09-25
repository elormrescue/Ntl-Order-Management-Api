using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ntl.AuthSS.Order_Management.Api.Models;
using Ntl.AuthSS.OrderManagement.Data.Entities;

namespace Ntl.AuthSS.Order_Management.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected OrderEntityType? GetOrderEntityType()
        {
            switch (GetEntityOrgType())
            {
                case "Manufacturer":
                    return OrderEntityType.Manufacturer;
                case "Tpsaf":
                    return OrderEntityType.Tpsaf;
                case "Ntl":
                    return OrderEntityType.Ntl;
                case "Shipper":
                    return OrderEntityType.Shipper;
                case "TaxAuthority":
                    return OrderEntityType.TaxAuth;
                default:
                    return null;
            }
        }

        protected string GetUserName()
        {
            return User.Claims.Single(x => x.Type == "GivenName").Value;
        }

        protected int GetEntityOrgId()
        {
            return Convert.ToInt32(User.Claims.Single(x => x.Type == "OrgId").Value);
        }

        protected string GetEntityRole()
        {
            return User.Claims.Single(x => x.Type == "Role").Value;
        }

        protected Roles GetEntityRoleType()
        {
            return (Roles)Enum.Parse(typeof(Roles), User.Claims.Single(x => x.Type == "Role").Value, true);
        }

        protected string GetEntityOrgType()
        {
            return User.Claims.Single(x => x.Type == "OrgType").Value;
        }

        protected int GetEntityLocation()
        {
            return Convert.ToInt32(User.Claims.Single(x => x.Type == "LocationId").Value);
        }

        protected int GetEntityId()
        {
            var entityIdClaim = User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (entityIdClaim != null)
                return Convert.ToInt32(entityIdClaim.Value);
            return 1;
        }
    }
}
