using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASC.Models.BaseTypes;
using ASC.Web.Areas.Accounts.Models;
using ASC.Web.Controllers;
using ASC.Web.Models;
using ASC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ASC.Utilities;
using System.Security.Claims;

namespace ASC.Web.Areas.Accounts.Controllers
{
    [Area("Accounts")]
    public class AccountController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ServiceEngineers()
        {
            var serviceEngineers = await _userManager.GetUsersInRoleAsync(Roles.Engineer.ToString());

            HttpContext.Session.SetSession("ServiceEngineers", serviceEngineers);
            foreach (var serviceEngineer in serviceEngineers)
            {
                serviceEngineer.Claims = (await _userManager.GetClaimsAsync(serviceEngineer)).ToList();
            }
            return View(new ServiceEngineerViewModel()
            {
                ServiceEngineers = serviceEngineers == null ? null : serviceEngineers.ToList(),
                Registration = new ServiceEngineerRegistrationViewModel() { IsEdit = false }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ServiceEngineers(ServiceEngineerViewModel serviceEngineer)
        {
            serviceEngineer.ServiceEngineers = HttpContext.Session.GetSession<List<ApplicationUser>>("ServiceEngineers");
            if (!ModelState.IsValid)
            {
                return View(serviceEngineer);
            }
            if (serviceEngineer.Registration.IsEdit)
            {
                var user = await _userManager.FindByEmailAsync(serviceEngineer.Registration.Email);
                user.UserName = serviceEngineer.Registration.UserName;
                var identityResult = await _userManager.UpdateAsync(user);
                if (!identityResult.Succeeded)
                {
                    identityResult.Errors.ToList().ForEach(e => ModelState.AddModelError("", e.Description));
                    return View(serviceEngineer);
                }
                // Update Password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                IdentityResult passwordResult = await _userManager.ResetPasswordAsync(user, token,
                serviceEngineer.Registration.Password);
                if (!passwordResult.Succeeded)
                {
                    passwordResult.Errors.ToList().ForEach(p => ModelState.AddModelError("",
                    p.Description));
                    return View(serviceEngineer);
                }
                // Update claims
                user = await _userManager.FindByEmailAsync(serviceEngineer.Registration.Email);
                user.Claims = (await _userManager.GetClaimsAsync(user)).ToList();
                var isActiveClaim = user.Claims.SingleOrDefault(p => p.Type == "IsActive");
                var removeClaimResult = await _userManager.RemoveClaimAsync(user, new System.Security.Claims.Claim(isActiveClaim.Type, isActiveClaim.Value));
                var addClaimResult = await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(isActiveClaim.Type, serviceEngineer.Registration.IsActive.ToString()));
            }
            else
            {
                // Create User
                ApplicationUser user = new ApplicationUser
                {
                    UserName = serviceEngineer.Registration.UserName,
                    Email = serviceEngineer.Registration.Email,
                    EmailConfirmed = true
                };
                IdentityResult result = await _userManager.CreateAsync(user, serviceEngineer.Registration.Password);
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", serviceEngineer.Registration.Email));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", serviceEngineer.Registration.IsActive.ToString()));
                if (!result.Succeeded)
                {
                    result.Errors.ToList().ForEach(p => ModelState.AddModelError("", p.Description));
                    return View(serviceEngineer);
                }
                // Assign user to Engineer Role
                var roleResult = await _userManager.AddToRoleAsync(user, Roles.Engineer.ToString());
                if (!roleResult.Succeeded)
                {
                    roleResult.Errors.ToList().ForEach(p => ModelState.AddModelError("", p.Description));
                    return View(serviceEngineer);
                }
            }
            if (serviceEngineer.Registration.IsActive)
            {
                await _emailSender.SendEmailAsync(serviceEngineer.Registration.Email,
                "Account Created/Modified", $"Email : {serviceEngineer.Registration.Email} /n Passowrd : {serviceEngineer.Registration.Password}");
            }
            else
            {
                await _emailSender.SendEmailAsync(serviceEngineer.Registration.Email,
                "Account Deactivated", $"Your account has been deactivated.");
            }
            return RedirectToAction("ServiceEngineers");
        }

        //[HttpGet]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Customers()
        //{
        //    // Code removed for brevity. This is moved from the other AccountController.
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Customers(CustomersViewModel customer)
        //{
        //    // Code removed for brevity. This is moved from the other AccountController.
        //}
        //[HttpGet]
        //public async Task<IActionResult> Profile()
        //{
        //    // Code removed for brevity. This is moved from the other AccountController.
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Profile(ProfileViewModel profile)
        //{
        //    // Code removed for brevity. This is moved from the other AccountController.
        //}

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}