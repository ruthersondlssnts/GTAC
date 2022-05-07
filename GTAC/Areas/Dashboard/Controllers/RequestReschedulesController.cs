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
            var applicationDbContext = _context.RequestReschedule.Include(r => r.Schedule);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Dashboard/RequestReschedules/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requestReschedule = await _context.RequestReschedule
                .Include(r => r.Schedule)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (requestReschedule == null)
            {
                return NotFound();
            }

            return View(requestReschedule);
        }

        // GET: Dashboard/RequestReschedules/Create
        public IActionResult Create()
        {
            ViewData["ScheduleId"] = new SelectList(_context.Schedules, "Id", "Id");
            return View();
        }

        // POST: Dashboard/RequestReschedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DayOne,DayTwo,DayThree,DayFour,ScheduleId,ApprovedAt,Status")] RequestReschedule requestReschedule)
        {
            if (ModelState.IsValid)
            {
                requestReschedule.Id = Guid.NewGuid();
                requestReschedule.CreatedAt = DateTime.Now;
                _context.Add(requestReschedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ScheduleId"] = new SelectList(_context.Schedules, "Id", "Id", requestReschedule.ScheduleId);
            return View(requestReschedule);
        }

        // GET: Dashboard/RequestReschedules/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requestReschedule = await _context.RequestReschedule.FindAsync(id);
            if (requestReschedule == null)
            {
                return NotFound();
            }
            ViewData["ScheduleId"] = new SelectList(_context.Schedules, "Id", "Id", requestReschedule.ScheduleId);
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
            ViewData["ScheduleId"] = new SelectList(_context.Schedules, "Id", "Id", requestReschedule.ScheduleId);
            return View(requestReschedule);
        }

        // GET: Dashboard/RequestReschedules/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requestReschedule = await _context.RequestReschedule
                .Include(r => r.Schedule)
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
            var requestReschedule = await _context.RequestReschedule.FindAsync(id);
            _context.RequestReschedule.Remove(requestReschedule);
            var sched = await _context.Schedules.FindAsync(requestReschedule.ScheduleId);
            sched.Status = Status.Approved;
            await _context.SaveChangesAsync();
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("Index", "Schedules");
        }

        private bool RequestRescheduleExists(Guid id)
        {
            return _context.RequestReschedule.Any(e => e.Id == id);
        }
    }
}
