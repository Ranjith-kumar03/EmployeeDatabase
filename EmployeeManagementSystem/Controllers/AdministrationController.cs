using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EmployeeManagementSystem.Controllers
{
  //  [Authorize(Roles = "Admin")]
 // [Authorize(Policy = "AdminRolePolicy")]

    [Route("[Controller]/[action]")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<AdministrationController> logger;

        public AdministrationController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager,
            ILogger<AdministrationController> logger)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult>  ManageUserClaims(string userID)
        {
            var user = await userManager.FindByIdAsync(userID);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with ID = {userID} cannot be found";
                return View("NotFound");
            }
            var existingUserClaims = await userManager.GetClaimsAsync(user);
            var model = new UserClaimsViewModel{
                UserId=userID,
                
            };

            foreach(Claim claim in ClaimStore.AllClaims)
            {
                UserClaim userclaim = new UserClaim
                {
                    ClaimType = claim.Type
                };
                if(existingUserClaims.Any(c=>c.Type==claim.Type && c.Value=="true"))
                {
                    userclaim.IsSelected = true;
                }
                model.claims.Add(userclaim);
            }
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with ID = {model.UserId} cannot be found";
                return View("NotFound");
            }
            var claims = await userManager.GetClaimsAsync(user);
            var result = await userManager.RemoveClaimsAsync(user, claims);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot Remove Users Existing Clains");
                return View(model);
            }
            result = await userManager.AddClaimsAsync(user,
                model.claims.Select(c => new Claim(c.ClaimType, c.IsSelected ? "true":"false")));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot Add Selected Clains to User");
                return View(model);
            }


            return RedirectToAction("EditUser", new { id = model.UserId });

        }









        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole identityRole = new IdentityRole
                {
                    Name = model.RoleName
                };
                IdentityResult result = await roleManager.CreateAsync(identityRole);
                if (result.Succeeded)
                {
                    return RedirectToAction("listroles", "Administration");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles;
            // return roles which is Iqueryable

            return View(roles);
        }

        [HttpGet]
        
        public async Task<IActionResult> EditRole(string Id)
        {
            var role = await roleManager.FindByIdAsync(Id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with ID = {Id} cannot be found";
                return View("NotFound");
            }

            var model = new EditRoleVieModel { Id = role.Id, Name = role.Name };
            foreach (var user in userManager.Users)
            {
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    model.UserNames.Add(user.UserName);
                }
            }
            return View(model);
        }


        [HttpPost]
        
        public async Task<IActionResult> EditRole(EditRoleVieModel model)
        {
            var role = await roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with ID = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                role.Name = model.Name;
                var result = await roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("listroles");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }




        }
        [HttpGet]

        public async Task<IActionResult> EditUsersInRole(string RoleId)//Model Binding parametrer from Query took from EditRole.cshtml
        {
            ViewBag.RoleId = RoleId;
            var role = await roleManager.FindByIdAsync(RoleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id ={RoleId} cannot be found";
                return View("NotFound");
            }
            //we want a list of UserRoleViewModel
            var model = new List<UserRoleViewModel>();
            foreach (var user in userManager.Users)
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserID = user.Id,
                    UserName = user.UserName,


                };

                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }
                model.Add(userRoleViewModel);

            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string RoleId)//Model Binding parametrer from Query "id" took from EditUsersInRole.cshtml
        {

            var role = await roleManager.FindByIdAsync(RoleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id ={RoleId} cannot be found";
                return View("NotFound");
            }
            for (int i = 0; i < model.Count; i++)
            {
                var user = await userManager.FindByIdAsync(model[i].UserID);
                IdentityResult result = null;
                if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && await userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }
                if (result.Succeeded)
                {
                    if (i < (model.Count - 1))
                    {
                        continue;
                    }
                    else
                    {
                        return RedirectToAction("EditRole", new { Id = RoleId });
                    }
                }
            }
            return RedirectToAction("EditRole", new { Id = RoleId });
        }
        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = userManager.Users;
            return View(users);
        }
        [HttpGet]

        public async Task<IActionResult> EditUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"user with Id={id} cannot be found";
                return View("Not Found");
            }
            var userClaims = await userManager.GetClaimsAsync(user);
            var user_Roles = await userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                City = user.City,
                Claims = userClaims.Select(c =>  c.Type + " : " +c.Value).ToList(),
                Roles = user_Roles


            };
            return View(model);
        }
        [HttpPost]

        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"user with Id={model.Id} cannot be found";
                return View("Not Found");
            }
            else
            {
                user.Email = model.Email;
                user.UserName = model.UserName;
                user.City = model.City;
                var result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
        [HttpPost]
       
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"user with Id={id} cannot be found";
                return View("Not Found");
            }
            else
            {
                var result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("listusers");

            }
        }

        [HttpPost]
       [Authorize(Policy = "DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id={id} cannot be found";
                return View("Not Found");
            }
            else
            {
                try
                {
                    // throw new Exception("Iam a fool");// will throw general Error Message
                    var result = await roleManager.DeleteAsync(role);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Listroles");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View("listroles");
                }
                catch (DbUpdateException e)
                {
                    logger.LogError($"Deleting Role {e}");
                    ViewBag.ErrorTitle = $"{role.Name} is in Use";
                    ViewBag.ErrorMessage = $"{role.Name} Role Cannot Be Deleted" +
                        $"as there are USERS in this Role If you want to Delete the Role" +
                        $"Delete Users Linked to This Role and Try Deleting !!!!!!!!!!! Sounds right";

                    return View("error");
                }

            }
        }
        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(string UserID)
        {
            ViewBag.UserId = UserID;
            var user = await userManager.FindByIdAsync(UserID);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id:{UserID} cannot be found";
                return View("NotFound");
            }
            var model = new List<UserRolesViewModel>();
            foreach (var role in roleManager.Roles)
            {
                var userRolesViewModel = new UserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name

                };
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.IsSelected = true;
                }
                else
                {
                    userRolesViewModel.IsSelected = false;
                }
                model.Add(userRolesViewModel);
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> model, string UserID)
        {
            var user = await userManager.FindByIdAsync(UserID);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id:{UserID} cannot be found";
                return View("NotFound");
            }
            var roles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user, roles);
            if(!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot Remove Users Existing Roles");
                return View(model);
            }
            result= await userManager.AddToRolesAsync(user, model.Where(x => x.IsSelected).Select(y => y.RoleName));
             if(!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add Selected Roles to User");
                return View(model);
            }
            return RedirectToAction("EditUser", new { id = UserID });
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
