using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace ASC.Utilities
{
    public class CurrentUser
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public string[] Roles { get; set; }
    }

    public static class ClaimsPrincipalExtensions
    {
        public static CurrentUser GetCurrentUser(this ClaimsPrincipal claimsPrincipal)
        {
            if (!claimsPrincipal.Claims.Any())
            {
                return null;
            }

            return new CurrentUser()
            {
                Name = claimsPrincipal.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault(),
                Email = claimsPrincipal.Claims.Where(c => c.Type == ClaimTypes.Email).Select(c => c.Value).SingleOrDefault(),
                Roles = claimsPrincipal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray(),
                IsActive = Boolean.Parse(claimsPrincipal.Claims.Where(c => c.Type == "IsActive").Select(c => c.Value).SingleOrDefault())
            };
        }
    }
}
