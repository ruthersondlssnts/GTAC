using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GTAC.Data;
using GTAC.Models;
using Microsoft.AspNetCore.Identity;

namespace GTAC.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class SchedulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public SchedulesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult GetDisabledDates()
        {
            var dates = _context.Schedules.Where(x => x.Date >= DateTime.Now && x.Status != Status.Reject)
                .GroupBy(x => x.Date)
                .Select(s => new { Date = s.Key, TotalSched = s.Count() })
                .AsEnumerable()
                .Where(s =>
                    (s.Date.DayOfWeek != DayOfWeek.Saturday && s.TotalSched == 4) ||
                    (s.Date.DayOfWeek == DayOfWeek.Saturday && s.TotalSched == 1))
                .ToList();

            return Json(new { dates });
        }

        public async Task<IActionResult> GetAvailableTime(DateTime date)
        {
            var times = await _context.Schedules.Where(x => x.Date.Date == date.Date && x.Status != Status.Reject)
                .GroupBy(x => x.Time)
                .Select(s => new { Time = s.Key })
                .ToListAsync();

            return Json(new { times });
        }


        // GET: Dashboard/Schedules
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Schedules.Include(s => s.Student).ThenInclude(s => s.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Dashboard/Schedules/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedules
                .Include(s => s.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        // GET: Dashboard/Schedules/Create
        public IActionResult Create()
        {
            //ViewData["StudentId"] = new SelectList(_context.Students, "Id", "UserId");
            return View();
        }

        // POST: Dashboard/Schedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,Time,Concerns")] Schedule schedule)
        {
            if (ModelState.IsValid)
            {
                schedule.Id = Guid.NewGuid();
                schedule.Status = Status.Pending;
                schedule.StudentId = _context.Students.Where(s => s.UserId == _userManager.GetUserId(User)).FirstOrDefault().Id;
                _context.Add(schedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(schedule);
        }

        // GET: Dashboard/Schedules/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }
            List<SelectListItem> timeSchedules = new List<SelectListItem>
            {
                new SelectListItem() { Value = "8am to 12pm" },
                new SelectListItem() { Value = "1pm to 5pm" }
            };
            ViewData["TimeSchedules"] = new SelectList(timeSchedules, "Value", "Value");
            return View(schedule);
        }

        // POST: Dashboard/Schedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Date,Time,StudentId,Concerns,ApprovedAt,Status")] Schedule schedule)
        {
            if (id != schedule.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (schedule.Status == Status.Approved)
                    {
                        schedule.ApprovedAt = DateTime.Now;
                    }
                    else if (schedule.Status == Status.Reject)
                    {
                        schedule.ApprovedAt = null;
                    }

                    _context.Update(schedule);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScheduleExists(schedule.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(schedule);
        }

        // GET: Dashboard/Schedules/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedules
                .Include(s => s.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        // POST: Dashboard/Schedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ScheduleExists(Guid id)
        {
            return _context.Schedules.Any(e => e.Id == id);
        }
    }
}
