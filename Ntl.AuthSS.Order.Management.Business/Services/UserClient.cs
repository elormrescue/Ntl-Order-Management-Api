using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Ntl.Tss.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System;


namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public class UserClient
    {
        private readonly HttpClient _client;
        private readonly TssIdentityDbContext _tssIdentityDbContext;
        public UserClient(HttpClient client, TssIdentityDbContext tssIdentityDbContext)
        {
            _client = client;
            _tssIdentityDbContext = tssIdentityDbContext;
        }

        public async Task<List<(string userName, string email)>> GetUsersEmailbyRole(List<string> orgRoles)
        {
            var usersList = new List<(string userName, string email)>();
            foreach (var role in orgRoles)
            {
                var users = await _tssIdentityDbContext.UserClaims.Include(u => u.User).Include(u => u.User.Claims).
                Where(u => u.ClaimType == "Role" && u.ClaimValue == role).Select(u => u.User).Where(u => u.IsActive).ToListAsync();
                foreach (var user in users)
                {
                    usersList.Add((userName: user.Claims.Where(u => u.ClaimType == "GivenName").Select(u => u.ClaimValue).SingleOrDefault(), email: user.Email));
                }
            }
            return usersList;
        }
        public async Task<List<(string userName, string Role, string email)>> GetUsersEmailbyOrgId(int orgId)
        {
            var usersList = new List<(string userName, string Role, string email)>();
            var userDetails = await _tssIdentityDbContext.UserClaims.Include(u => u.User).Include(u => u.User.Claims).Where(u => u.ClaimType == "OrgId" && u.ClaimValue == orgId.ToString() && u.User.IsActive).Select(u => u.User).ToListAsync();
            foreach (var user in userDetails)
                usersList.Add((userName: user.Claims.Where(u => u.ClaimType == "GivenName").Select(u => u.ClaimValue).SingleOrDefault(), Role: user.Claims.Where(u => u.ClaimType == "Role").Select(u => u.ClaimValue).SingleOrDefault(), email: user.Email));
            return usersList;
        }
        public async Task<List<(string userName, string Role, string email)>> GetUsersEmailbyOrgIdandOrgRole(int orgId, string orgRole)
        {
            var usersList = new List<(string userName, string Role, string email)>();
            var userDetails = _tssIdentityDbContext.UserClaims.Include(u => u.User).Include(u => u.User.Claims).Where(u => u.ClaimType == "OrgId" && u.ClaimValue == orgId.ToString() && u.User.IsActive).Select(u => u.User);
            userDetails = userDetails.Where(u => u.Claims.SingleOrDefault(u => u.ClaimType == "Role").ClaimValue == orgRole);
            foreach (var user in userDetails)
                usersList.Add((userName: user.Claims.SingleOrDefault(u => u.ClaimType == "GivenName").ClaimValue, Role: user.Claims.SingleOrDefault(u => u.ClaimType == "Role").ClaimValue, email: user.Email));
            return usersList;
        }
        public async Task<(string UserName, string Role, string EmailId)> GetEmailByUserId(int userId)
        {
            var user = await _tssIdentityDbContext.Users.Include(u => u.Claims).Where(u => u.Id == userId).SingleOrDefaultAsync();
            return (user.Claims.Where(u => u.ClaimType == "GivenName").Select(u => u.ClaimValue).SingleOrDefault(), user.Claims.Where(u => u.ClaimType == "Role").Select(u => u.ClaimValue).SingleOrDefault(), user.Email);
        }
        public async Task<List<UserDevice>> GetUserDevices(int shipperId)
        {
            var userDevices = await _tssIdentityDbContext.UserClaims.Include(u => u.User.Claims).Where(u => u.ClaimType == "OrgId" && u.ClaimValue == shipperId.ToString()).Select(u => u.User).Where(x => x.IsActive).SelectMany(x => x.UserDevices).Where(x => x.AppToken != null & x.IsActive).ToListAsync();
            return userDevices;
        }
        public async Task<List<string>> GetAdminRoles()
        {
            var adminRoles = new List<string> { "TpsafAdmin", "TpsafFacilityAdmin", "TpsafFacilityIncharge", "TaxAuthAdmin", "TaxAuthRevenueOfficer" };
            var adminRolesWithOrgId = new List<string>();
            var users = _tssIdentityDbContext.UserClaims.Where(u => u.ClaimType == "Role" && adminRoles.Contains(u.ClaimValue)).Select(u => new { OrgId = u.User.Claims.Where(u => u.ClaimType == "OrgId").Select(u => u.ClaimValue).SingleOrDefault(), Role = u.User.Claims.Where(u => u.ClaimType == "Role").Select(u => u.ClaimValue).SingleOrDefault() }).Distinct();
            foreach (var user in users)
                adminRolesWithOrgId.Add(user.OrgId + "-" + user.Role);
            return adminRolesWithOrgId;
        }
        public async Task<List<string>> GetOrgUserRoles(string orgId)
        {
            var orgUserRolesWithOrgId = new List<string>();
            var users = _tssIdentityDbContext.UserClaims.Where(u => u.ClaimType == "OrgId" && u.ClaimValue == orgId).Select(u => new { OrgId = u.User.Claims.Where(u => u.ClaimType == "OrgId").Select(u => u.ClaimValue).SingleOrDefault(), Role = u.User.Claims.Where(u => u.ClaimType == "Role").Select(u => u.ClaimValue).SingleOrDefault() }).Distinct();
            foreach (var user in users)
                orgUserRolesWithOrgId.Add(user.OrgId + "-" + user.Role);
            return orgUserRolesWithOrgId;
        }
        public async Task<List<(int id, string userRole, string orgId)>> GetAdminUserIdsByRoles(List<string> orgRoles)
        {
            var usersList = new List<(int id, string userRole)>();
            var users = _tssIdentityDbContext.UserClaims.Include(u => u.User).Include(u => u.User.Claims).Where(u => orgRoles.Contains(u.ClaimValue) && u.User.IsActive);
            return users.Select(u => new { Id = u.Id, Role = u.User.Claims.SingleOrDefault(user => user.ClaimType == "Role").ClaimValue, OrgId = u.User.Claims.SingleOrDefault(user => user.ClaimType == "OrgId").ClaimValue }).AsEnumerable().Select(u => (u.Id, u.Role, u.OrgId)).ToList();
        }
        public async Task<List<(int id, string userRole, string orgId)>> GetOrgUserIdsByRoleOrgId(List<string> orgRoles, string orgId)
        {
            var usersList = new List<(int id, string userRole)>();
            var users = _tssIdentityDbContext.UserClaims.Include(u => u.User).Include(u => u.User.Claims).Where(u => u.ClaimType == "OrgId" && u.ClaimValue == orgId && u.User.IsActive);
            users = users.Where(u => orgRoles.Contains(u.ClaimValue));
            return users.Select(u => new { Id = u.Id, Role = u.User.Claims.SingleOrDefault(user => user.ClaimType == "Role").ClaimValue, OrgId = u.User.Claims.SingleOrDefault(user => user.ClaimType == "OrgId").ClaimValue }).AsEnumerable().Select(u => (u.Id, u.Role, u.OrgId)).ToList();
        }
    }
}
