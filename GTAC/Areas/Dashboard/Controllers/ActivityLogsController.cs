using GTAC.Data;
using GTAC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GTAC.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class ActivityLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string dateParam = null, string actionParam = null, string areaParam = null)
        {
            dateParam = dateParam ?? DateTime.Now.Date.ToShortDateString();
            ViewBag.date = dateParam;
            ViewBag.action = actionParam;
            ViewBag.area = areaParam;

            var activityLogs = _context
                 .ActivityLogs
                 .Include(q => q.User)
                 .Where(a => actionParam == null ? true : (int)a.Action == int.Parse(actionParam))
                 .Where(a => areaParam == null ? true : a.Area == (Area)int.Parse(areaParam))
                 .Where(a => a.CreatedAt.Date == DateTime.Parse(dateParam).Date)
                 .OrderBy(a => a.CreatedAt);

            return View(await activityLogs.ToListAsync());
        }
    }
}
