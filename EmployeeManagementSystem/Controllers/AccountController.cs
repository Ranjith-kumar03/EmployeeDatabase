using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EmployeeManagementSystem.Controllers
{
    [Route("[Controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signManager)
        {
            this.userManager = userManager;
            this.signManager = signManager;
        }

        //  [HttpGet][HttpPost] // we can use this or we can use another  [AcceptVerbs("Get","Post")]
        [AcceptVerbs("Get","Post")]
        [AllowAnonymous]

        public async Task<IActionResult> IsEmailInUse(string email)
        {
           var user= await userManager.FindByEmailAsync(email);
            if(user==null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Email id {email} already in use");
            }
        }
        // GET: /<controller>/
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        //If you declare method Asynch the return type should be Task<returntype>
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            //From the Validation Check
            if(ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email,City=model.City };
               var result=await userManager.CreateAsync(user, model.Password);
                if(result.Succeeded)
                {
                    if(signManager.IsSignedIn(User)&&User.IsInRole("Admin"))
                    {
                        return RedirectToAction("ListUsers", "administration");
                    }
                    //2nd parametr asks for Session cookie or permanent cooke
                    // we go for session cookie
                   await  signManager.SignInAsync(user, isPersistent: false);// isPersistion session should not be persistient
                    return RedirectToAction("index", "home");
                }
                foreach( var error in result.Errors)
                {
                    ModelState.AddModelError("",error.Description);
                }
                    
            }
            return View(model);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            LogInViewModel model = new LogInViewModel()
            {
                ReturnUrl = returnUrl,
                externalLogIns = (await signManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            
            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LogInViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {

                var result = await signManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    if(!string.IsNullOrEmpty(returnUrl)&&Url.IsLocalUrl(returnUrl))
                    {
                        //now wecan use redirect
                        return Redirect(returnUrl);

                    }
                    else
                    {
                        return RedirectToAction("index", "home");
                    }
                                       
                    
                }
               
                    ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
                
            }
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
           await  signManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        [HttpPost]
        [AllowAnonymous]
        public  IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallBack", "Account", new { ReturnUrl = returnUrl });

            var properties = signManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

    }
}
