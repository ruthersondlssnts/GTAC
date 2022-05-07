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

        public SchedulesController(
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult GetDisabledDates(string id = null)
        {
            Guid Id = id == null ? Guid.NewGuid() : new Guid(id);

            var dates1 = _context.Schedules.Where(x => x.Status != Status.Reject && x.Status != Status.Done && x.Id != Id)
                .GroupBy(x => x.DayOne.Date)
                .Select(s => new { Date = s.Key, TotalSched = s.Count() })
                .AsEnumerable()
                .Where(s => s.Date.DayOfWeek != DayOfWeek.Saturday && s.Date.DayOfWeek != DayOfWeek.Sunday)
                .ToList();

            var dates2 = _context.Schedules.Where(x => x.Status != Status.Reject && x.Status != Status.Done && x.Id != Id)
                .GroupBy(x => x.DayTwo.Date)
                .Select(s => new { Date = s.Key, TotalSched = s.Count() })
                .AsEnumerable()
                .Where(s => s.Date.DayOfWeek != DayOfWeek.Saturday && s.Date.DayOfWeek != DayOfWeek.Sunday)
                .ToList();

            var dates3 = _context.Schedules.Where(x => x.Status != Status.Reject && x.Status != Status.Done && x.Id != Id)
                .GroupBy(x => x.DayThree.Date)
                .Select(s => new { Date = s.Key, TotalSched = s.Count() })
                .AsEnumerable()
                .Where(s => s.Date.DayOfWeek != DayOfWeek.Saturday && s.Date.DayOfWeek != DayOfWeek.Sunday)
                .ToList();

            var dates4 = _context.Schedules.Where(x => x.Status != Status.Reject && x.Status != Status.Done && x.Id != Id)
                .GroupBy(x => x.DayFour.Date)
                .Select(s => new { Date = s.Key, TotalSched = s.Count() })
                .AsEnumerable()
                .Where(s => s.Date.DayOfWeek != DayOfWeek.Saturday && s.Date.DayOfWeek != DayOfWeek.Sunday)
                .ToList();

            dates1.AddRange(dates2);
            dates1.AddRange(dates3);
            dates1.AddRange(dates4);

            var dates = dates1.GroupBy(x => x.Date).Select(x => new { Date = x.Key, Total = x.Sum(x => x.TotalSched) }).Where(x => x.Total > 3).Select(x => x.Date).ToList();

            return Json(new { dates });
        }

        public async Task<IActionResult> GetDisabledTime(DateTime date, string id = null)
        {
            Guid Id = id == null ? Guid.NewGuid() : new Guid(id);

            var times = (await _context.Schedules.Where(x => (
                    x.DayOne.Date == date.Date ||
                    x.DayTwo.Date == date.Date ||
                    x.DayThree.Date == date.Date ||
                    x.DayFour.Date == date.Date) && (
                    x.Status != Status.Reject &&
                    x.Id != Id &&
                    x.Status != Status.Done))
                .Select(x => new
                {
                    Date = x.DayOne.Date == date.Date ? x.DayOne : (x.DayTwo.Date == date.Date ? x.DayTwo :
                        (x.DayThree.Date == date.Date ? x.DayThree : x.DayFour))
                })
                .ToListAsync()).Select(x => x.Date.ToString("h:mm tt")).ToList();

            return Json(new { times });
        }

        // GET: Dashboard/Schedules
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                var applicationDbContext = _context.Schedules.Include(s => s.Student).ThenInclude(s => s.User);
                return View(await applicationDbContext.ToListAsync());
            }
            else
            {
                var studentSchedule = await _context.Schedules
                    .Include(s => s.Student)
                    .ThenInclude(s => s.User)
                    .Where(s => s.Student.UserId == _userManager.GetUserId(User))
                    .FirstOrDefaultAsync();

                if (studentSchedule == null)
                {
                    return View("Create");
                }

                return View("StudentSchedule", studentSchedule);
            }
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
                .ThenInclude(s => s.User)
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
        public async Task<IActionResult> Create([Bind("Id,DayOne,DayTwo,DayThree,DayFour,ApprovedAt")] Schedule schedule)
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
            return View(schedule);
        }

        // POST: Dashboard/Schedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,DayOne,DayTwo,DayThree,DayFour,StudentId,ApprovedAt,Status")] Schedule schedule)
        {
            if (id != schedule.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    schedule.ApprovedAt = DateTime.Now;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChangeRequest([Bind("Id,DayOne,DayTwo,DayThree,DayFour,StudentId,ApprovedAt,Status")] Schedule schedule)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    RequestReschedule requestReschedule = new RequestReschedule();
                    requestReschedule.Id = Guid.NewGuid();
                    requestReschedule.ScheduleId = schedule.Id;
                    requestReschedule.DayOne = schedule.DayOne;
                    requestReschedule.DayTwo = schedule.DayTwo;
                    requestReschedule.DayThree = schedule.DayThree;
                    requestReschedule.DayFour = schedule.DayFour;
                    requestReschedule.Status = Status.Pending;
                    requestReschedule.CreatedAt = DateTime.Now;
                    var prevsched = await _context.Schedules.FindAsync(schedule.Id);
                    prevsched.Status = Status.PendingRequest;
                    _context.Add(requestReschedule);
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
                .ThenInclude(s => s.User)
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
