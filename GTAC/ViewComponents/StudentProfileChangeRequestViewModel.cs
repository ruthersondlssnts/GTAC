using GTAC.Data;
using GTAC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GTAC.ViewComponents
{
    public class StudentProfileChangeRequestViewModel : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public StudentProfileChangeRequestViewModel(
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IViewComponentResult Invoke()
        {
            var id = _userManager.GetUserId((System.Security.Claims.ClaimsPrincipal)User);
            var studentsRequestChange = _context.StudentChangeRequests
                .Include(x => x.User)
                .Where(x => x.UserId == id)
                .FirstOrDefault();

            if (studentsRequestChange != null && studentsRequestChange.Status == Status.Approved)
            {
                studentsRequestChange = null;
            }
            else
            {
                ActivityLog.Create(_userManager.GetUserId((System.Security.Claims.ClaimsPrincipal)User), Area.RequestProfileChange, Models.Action.View, "Viewed his/her Request Profile Change", _context);
            }
            return View(studentsRequestChange);
        }
    }
}
