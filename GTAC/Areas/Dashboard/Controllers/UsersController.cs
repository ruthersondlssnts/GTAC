using GTAC.Data;
using GTAC.Models;
using GTAC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GTAC.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UsersController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        // GET: UsersController
        public ActionResult Index()
        {
            var users = (from user in _context.Users
                         join user_role in _context.UserRoles on user.Id equals user_role.UserId
                         join role in _context.Roles on user_role.RoleId equals role.Id
                         select new UserViewModel { User = user, Role = role.Name }).ToList();

            ActivityLog.Create(_userManager.GetUserId(User), Area.CompanyUser, Models.Action.View, "Viewed company users", _context);

            return View(users);
        }
        // GET: Dashboard/Students/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            var role = await _userManager.GetRolesAsync(user);

            UserViewModel userViewModel = new UserViewModel
            {
                User = user,
                Role = role.FirstOrDefault()
            };

            if (user == null)
            {
                return NotFound();
            }

            ActivityLog.Create(_userManager.GetUserId(User), Area.CompanyUser, Models.Action.View, "Viewed a company user " + user.Firstname + " " + user.Lastname, _context);

            return View(userViewModel);
        }

        // GET: UsersController/Create
        [HttpGet]
        public ActionResult Create()
        {
            ViewData["Roles"] = new SelectList(_roleManager.Roles, "Name", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel user)
        {
            if (ModelState.IsValid)
            {
                User appUser = new User
                {
                    UserName = user.Email,
                    Email = user.Email,
                    Firstname = user.Firstname,
                    Lastname = user.Lastname,
                    Middlename = user.Middlename,
                    Suffix = user.Suffix,
                    Address = user.Address,
                    Birthday = Convert.ToDateTime(user.Birthday),
                    PhoneNumber = user.PhoneNumber
                };

                IdentityResult result = await _userManager.CreateAsync(appUser, user.Password);

                if (result.Succeeded)
                {
                    ActivityLog.Create(_userManager.GetUserId(User), Area.CompanyUser, Models.Action.Create, "Created a company user", _context);
                    result = await _userManager.AddToRoleAsync(appUser, user.Role);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            ViewData["Roles"] = new SelectList(_roleManager.Roles, "Name", "Name");
            return View(user);

        }


        // GET: UsersController/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var role = await _userManager.GetRolesAsync(user);
            EditUserViewModel uvm = new EditUserViewModel
            {
                Email = user.Email,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Middlename = user.Middlename,
                Suffix = user.Suffix,
                Address = user.Address,
                Birthday = user.Birthday.ToString(),
                PhoneNumber = user.PhoneNumber,
                Role = role.FirstOrDefault(),
                Id = user.Id
            };

            ViewData["Roles"] = new SelectList(_roleManager.Roles, "Name", "Name");

            return View(uvm);
        }

        // POST: UsersController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel uvm)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(uvm.Id);
                user.UserName = uvm.Email;
                user.Email = uvm.Email;
                user.Firstname = uvm.Firstname;
                user.Lastname = uvm.Lastname;
                user.Middlename = uvm.Middlename;
                user.Suffix = uvm.Suffix;
                user.Address = uvm.Address;
                user.Birthday = Convert.ToDateTime(uvm.Birthday);
                user.PhoneNumber = uvm.PhoneNumber;

                IdentityResult result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    var userRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                    ActivityLog.Create(_userManager.GetUserId(User), Area.CompanyUser, Models.Action.Edit, "Edited a company user " + user.Firstname + " " + user.Lastname, _context);

                    if (userRole == uvm.Role)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    result = await _userManager.RemoveFromRoleAsync(user, userRole);
                    result = await _userManager.AddToRoleAsync(user, uvm.Role);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                ViewData["Roles"] = new SelectList(_roleManager.Roles, "Name", "Name");
                return View(user);
            }
            catch
            {
                return View();
            }
        }

        // GET: UsersController/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            var role = await _userManager.GetRolesAsync(user);

            UserViewModel userViewModel = new UserViewModel
            {
                User = user,
                Role = role.FirstOrDefault()
            };

            if (user == null)
            {
                return NotFound();
            }

            return View(userViewModel);
        }

        // POST: Dashboard/Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                ActivityLog.Create(_userManager.GetUserId(User), Area.CompanyUser, Models.Action.Delete, "Deleted a company user " + user.Firstname + " " + user.Lastname, _context);
                IdentityResult result = await _userManager.DeleteAsync(user);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

    }
}
