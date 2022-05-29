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
using Microsoft.AspNetCore.Authorization;

namespace GTAC.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class RequestReschedulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public RequestReschedulesController(ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Dashboard/RequestReschedules

        public async Task<IActionResult> Index()
        {
            ActivityLog.Create(_userManager.GetUserId(User), Area.RequestReschedule, Models.Action.View, "Viewed Requests for Reschedules", _context);

            var applicationDbContext = _context.RequestReschedules
                .Include(r => r.Schedule)
                .ThenInclude(s => s.Student)
                .ThenInclude(s => s.User)
                .OrderByDescending(r => r.CreatedAt);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Dashboard/RequestReschedules
        public async Task<IActionResult> TotalPendingRequests()
        {
            var applicationDbContext = await _context.RequestReschedules.CountAsync(r => r.Status == Status.Pending);

            return Json(new { totalRequests = applicationDbContext });
        }


        // GET: Dashboard/RequestReschedules/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requestReschedule = await _context.RequestReschedules
                .Include(r => r.Schedule)
                .ThenInclude(s => s.Student)
                .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            ActivityLog.Create(_userManager.GetUserId(User),
                Area.RequestReschedule, Models.Action.View,
                "Viewed a Request for Reschedule of "
                + requestReschedule.Schedule.Student.User.Firstname + " "
                + requestReschedule.Schedule.Student.User.Firstname,
                _context);

            if (requestReschedule == null)
            {
                return NotFound();
            }

            return View(requestReschedule);
        }

        // GET: Dashboard/RequestReschedules/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requestReschedule = await _context.RequestReschedules.FindAsync(id);
            if (requestReschedule == null)
            {
                return NotFound();
            }
            return View(requestReschedule);
        }

        // POST: Dashboard/RequestReschedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,DayOne,DayTwo,DayThree,DayFour,ScheduleId,CreatedAt,ApprovedAt,Status")] RequestReschedule requestReschedule)
        {
            if (id != requestReschedule.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var schedule = await _context.Schedules.FindAsync(requestReschedule.ScheduleId);

                    if (requestReschedule.Status == Status.Approved)
                    {
                        schedule.DayOne = requestReschedule.DayOne;
                        schedule.DayTwo = requestReschedule.DayTwo;
                        schedule.DayThree = requestReschedule.DayThree;
                        schedule.DayFour = requestReschedule.DayFour;
                        schedule.ApprovedAt = DateTime.Now;
                        requestReschedule.ApprovedAt = DateTime.Now;
                    }
                    schedule.Status = Status.Approved;
                    _context.Update(requestReschedule);
                    ActivityLog.Create(_userManager.GetUserId(User), Area.RequestReschedule, Models.Action.Edit, "Edited Request for Reschedule status to " + requestReschedule.Status, _context);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RequestRescheduleExists(requestReschedule.Id))
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
            return View(requestReschedule);
        }

        // GET: Dashboard/RequestReschedules/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requestReschedule = await _context.RequestReschedules
                .Include(r => r.Schedule)
                .ThenInclude(s => s.Student)
                .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (requestReschedule == null)
            {
                return NotFound();
            }

            return View(requestReschedule);
        }

        // POST: Dashboard/RequestReschedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var requestReschedule = await _context.RequestReschedules.FindAsync(id);
            _context.RequestReschedules.Remove(requestReschedule);
            var sched = await _context.Schedules.FindAsync(requestReschedule.ScheduleId);
            sched.Status = Status.Approved;
            await _context.SaveChangesAsync();
            ActivityLog.Create(_userManager.GetUserId(User), Area.RequestReschedule, Models.Action.Delete, "Deleted Request for Reschedule", _context);
            return RedirectToAction("Index", "Schedules");
        }

        private bool RequestRescheduleExists(Guid id)
        {
            return _context.RequestReschedules.Any(e => e.Id == id);
        }
    }
}
