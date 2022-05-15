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
    public class UserScheduleChangeRequestViewModel : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserScheduleChangeRequestViewModel(
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IViewComponentResult Invoke()
        {
            var id = _userManager.GetUserId((System.Security.Claims.ClaimsPrincipal)User);
            var studentsRequestChange = _context.RequestReschedules
                .Include(x => x.Schedule)
                .ThenInclude(x => x.Student)
                .ThenInclude(x => x.User)
                .Where(r => r.Schedule.Student.User.Id == id)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            if (studentsRequestChange != null && studentsRequestChange.Status == Status.Approved)
            {
                studentsRequestChange = null;
            }
            return View(studentsRequestChange);
        }
    }
}
