using ASC.Web.Configuration;
using ASC.Web.Models;
using ASC.Models.BaseTypes;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECM = ElCamino.AspNetCore.Identity.AzureTable.Model;
using System.Security.Claims;

namespace ASC.Web.Data
{
    public class IdentitySeed : IIdentitySeed
    {
        public async Task Seed(UserManager<ApplicationUser> userManager, RoleManager<ECM.IdentityRole> roleManager, IOptions<ApplicationSettings> options)
        {
            var roles = options.Value.Roles.Split(new char[] { ',' });
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ECM.IdentityRole(role));
                }
            }

            //Create Admin user
            var admin = await userManager.FindByNameAsync(options.Value.AdminName);
            if (admin == null)
            {
                ApplicationUser applicationAdmin = new ApplicationUser() { UserName = options.Value.AdminName, Email = options.Value.AdminEmail, EmailConfirmed = true, LockoutEnabled = false };
                IdentityResult result = await userManager.CreateAsync(applicationAdmin, options.Value.AdminPassword);
                await userManager.AddClaimAsync(applicationAdmin, new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", options.Value.AdminEmail));
                await userManager.AddClaimAsync(applicationAdmin, new Claim("IsActive", "True"));

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(applicationAdmin, Roles.Admin.ToString());
                }
            }

            //Create TestEngineer user
            var engineer = await userManager.FindByNameAsync(options.Value.EngineerName);
            if (engineer == null)
            {
                ApplicationUser applicationEngineer = new ApplicationUser() { UserName = options.Value.EngineerName, Email = options.Value.EngineerEmail, EmailConfirmed = true, LockoutEnabled = false };
                IdentityResult result = await userManager.CreateAsync(applicationEngineer, options.Value.EngineerPassword);
                await userManager.AddClaimAsync(applicationEngineer, new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", options.Value.EngineerEmail));
                await userManager.AddClaimAsync(applicationEngineer, new Claim("IsActive", "True"));

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(applicationEngineer, Roles.Engineer.ToString());
                }
            }
        }
    }
}
