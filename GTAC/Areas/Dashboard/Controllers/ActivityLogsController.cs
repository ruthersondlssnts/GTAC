using AspNetCore.Reporting;
using GTAC.Data;
using GTAC.Helpers;
using GTAC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
    [Authorize(Roles = "Admin,Manager")]
    public class ActivityLogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ActivityLogsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string dateFromParam = null, string dateToParam = null, string actionParam = null, string areaParam = null)
        {
            dateFromParam = dateFromParam ?? DateTime.Now.Date.ToShortDateString();
            dateToParam = dateToParam ?? DateTime.Now.Date.ToShortDateString();
            ViewBag.dateFromParam = dateFromParam;
            ViewBag.dateToParam = dateToParam;
            ViewBag.action = actionParam;
            ViewBag.area = areaParam;

            var activityLogs = _context
                 .ActivityLogs
                 .Include(q => q.User)
                 .Where(a => actionParam == null ? true : (int)a.Action == int.Parse(actionParam))
                 .Where(a => areaParam == null ? true : a.Area == (Area)int.Parse(areaParam))
                 .Where(a => a.CreatedAt.Date >= DateTime.Parse(dateFromParam).Date)
                 .Where(a => a.CreatedAt.Date <= DateTime.Parse(dateToParam).Date)
                 .OrderBy(a => a.CreatedAt);

            return View(await activityLogs.ToListAsync());
        }

        public IActionResult Print(string dateFromParam = null, string dateToParam = null, string actionParam = null, string areaParam = null)
        {
            dateFromParam = dateFromParam ?? DateTime.Now.Date.ToShortDateString();
            dateToParam = dateToParam ?? DateTime.Now.Date.ToShortDateString();

            var activityLogs = _context
                 .ActivityLogs
                 .Include(q => q.User)
                 .Where(a => actionParam == null ? true : (int)a.Action == int.Parse(actionParam))
                 .Where(a => areaParam == null ? true : a.Area == (Area)int.Parse(areaParam))
                 .Where(a => a.CreatedAt.Date >= DateTime.Parse(dateFromParam).Date)
                 .Where(a => a.CreatedAt.Date <= DateTime.Parse(dateToParam).Date)
                 .OrderBy(a => a.CreatedAt)
                 .Select(a => new
                 {
                     Account = a.User.Firstname + " " + a.User.Lastname,
                     Area = EnumExtensions.GetDisplayName(a.Area),
                     Action = a.Description,
                     Time = a.CreatedAt
                 })
                 .ToList();


            string minType = "";
            int extension = 1;
            var path = $"{this._webHostEnvironment.WebRootPath}\\Reports\\ActivityLogsReport.rdlc";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("from", dateFromParam);
            parameters.Add("to", dateToParam);
            LocalReport localReport = new LocalReport(path);
            localReport.AddDataSource("DataSet1", activityLogs);
            var result = localReport.Execute(RenderType.Pdf, extension, parameters, minType);
            return File(result.MainStream, "application/pdf");
        }
    }
}
