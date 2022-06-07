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
using GTAC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace GTAC.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
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
        public async Task<IActionResult> UpdateAssessment(Guid id, int day, bool? status)
        {
            var applicationDbContext = await _context.Schedules.Where(s => s.Id == id).FirstOrDefaultAsync();

            if (applicationDbContext != null)
            {
                switch (day)
                {
                    case 1:
                        applicationDbContext.isDayOnePassed = status;
                        break;
                    case 2:
                        applicationDbContext.isDayTwoPassed = status;
                        break;
                    case 3:
                        applicationDbContext.isDayThreePassed = status;
                        break;
                    case 4:
                        applicationDbContext.isDayFourPassed = status;
                        break;
                    default:
                        break;
                }
            }
            _context.Update(applicationDbContext);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // GET: Dashboard/RequestReschedules
        public async Task<IActionResult> TotalPendingRequests()
        {
            var applicationDbContext = await _context.Schedules.CountAsync(r => r.Status == Status.Pending);

            return Json(new { totalRequests = applicationDbContext });
        }



        public IActionResult GetDisabledDates(string studentId, string id = null)
        {
            Guid Id = id == null ? Guid.NewGuid() : new Guid(id);
            var instructorId = "";
            if (studentId == null)
            {
                instructorId = _context.Students.Where(s => s.UserId == _userManager.GetUserId(User)).FirstOrDefault().InstructorId;
            }
            else
            {
                instructorId = _context.Students.Where(s => s.Id == new Guid(studentId)).FirstOrDefault().InstructorId;
            }

            var dates1 = _context.Schedules
                .Include(x => x.Student)
                .Where(x => x.Status != Status.Reject &&
                    x.Status != Status.Done &&
                    x.Id != Id &&
                    x.Student.InstructorId == instructorId &&
                    x.DayOne.Date >= DateTime.Now.Date)
                .GroupBy(x => x.DayOne.Date)
                .Select(s => new { Date = s.Key, TotalSched = s.Count() })
                .ToList();

            var dates2 = _context.Schedules
                .Include(x => x.Student)
                .Where(x => x.Status != Status.Reject &&
                    x.Status != Status.Done &&
                    x.Id != Id &&
                    x.Student.InstructorId == instructorId &&
                    x.DayTwo.Date >= DateTime.Now.Date)
                .GroupBy(x => x.DayTwo.Date)
                .Select(s => new { Date = s.Key, TotalSched = s.Count() })
                .ToList();

            var dates3 = _context.Schedules
                .Include(x => x.Student)
                .Where(x => x.Status != Status.Reject &&
                    x.Status != Status.Done &&
                    x.Id != Id &&
                    x.Student.InstructorId == instructorId &&
                    x.DayThree.Date >= DateTime.Now.Date)
                .GroupBy(x => x.DayThree.Date)
                .Select(s => new { Date = s.Key, TotalSched = s.Count() })
                .ToList();

            var dates4 = _context.Schedules
                .Include(x => x.Student)
                .Where(x => x.Status != Status.Reject &&
                    x.Status != Status.Done &&
                    x.Id != Id &&
                    x.Student.InstructorId == instructorId &&
                    x.DayFour.Date >= DateTime.Now.Date)
                .GroupBy(x => x.DayFour.Date)
                .Select(s => new { Date = s.Key, TotalSched = s.Count() })
                .ToList();

            dates1.AddRange(dates2);
            dates1.AddRange(dates3);
            dates1.AddRange(dates4);

            var dates = dates1.GroupBy(x => x.Date).Select(x => new { Date = x.Key.ToString("MM/dd/yyyy"), Total = x.Sum(x => x.TotalSched) }).ToList();

            return Json(new { dates });
        }

        public async Task<IActionResult> GetDisabledTime(DateTime date, string studentId, string id = null)
        {
            Guid Id = id == null ? Guid.NewGuid() : new Guid(id);

            var instructorId = "";
            if (studentId == null)
            {
                instructorId = _context.Students.Where(s => s.UserId == _userManager.GetUserId(User)).FirstOrDefault().InstructorId;
            }
            else
            {
                instructorId = _context.Students.Where(s => s.Id == new Guid(studentId)).FirstOrDefault().InstructorId;
            }


            var times = (await _context.Schedules
                .Include(x => x.Student)
                .Where(x => (
                    x.DayOne.Date == date.Date ||
                    x.DayTwo.Date == date.Date ||
                    x.DayThree.Date == date.Date ||
                    x.DayFour.Date == date.Date) && (
                    x.Status != Status.Reject &&
                    x.Id != Id &&
                    x.Status != Status.Done &&
                    x.Student.InstructorId == instructorId))
                .ToListAsync());

            var times_disabled = new List<string>();
            foreach (var time in times)
            {
                if (time.DayOne.Date == date.Date)
                {
                    times_disabled.Add(time.DayOne.ToString("h:mm tt"));
                }
                if (time.DayTwo.Date == date.Date)
                {
                    times_disabled.Add(time.DayTwo.ToString("h:mm tt"));
                }
                if (time.DayThree.Date == date.Date)
                {
                    times_disabled.Add(time.DayThree.ToString("h:mm tt"));
                }
                if (time.DayFour.Date == date.Date)
                {
                    times_disabled.Add(time.DayFour.ToString("h:mm tt"));
                }
            }
            return Json(new { times = times_disabled });
        }

        // GET: Dashboard/Schedules
        //Dashboard/Schedules/
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin") || User.IsInRole("Staff") || User.IsInRole("Manager"))
            {
                ActivityLog.Create(_userManager.GetUserId(User), Area.Schedule, Models.Action.View, "Viewed Schedules", _context);
                var applicationDbContext = _context.Schedules.Include(s => s.Student).ThenInclude(s => s.User).Include(s => s.Student).ThenInclude(s => s.Instructor);
                return View(await applicationDbContext.ToListAsync());
            }
            else if (User.IsInRole("Instructor"))
            {
                var schedulesToday2 = await _context.Schedules
                  .Include(s => s.Student)
                  .ThenInclude(s => s.User)
                  .Where(s => s.Student.InstructorId == _userManager.GetUserId(User))
                  .Where(s => s.Status == Status.Approved)
                  .Where(s => s.DayOne.Date == DateTime.Now.Date || s.DayTwo.Date == DateTime.Now.Date || s.DayThree.Date == DateTime.Now.Date || s.DayFour.Date == DateTime.Now.Date)
                  .ToListAsync();

                var instructorSchedule = new InstructorScheduleViewModel();

                foreach (var st in schedulesToday2)
                {
                    if (st.DayOne.Date == DateTime.Now.Date)
                    {
                        if (st.DayOne.Hour == 7)
                        {
                            instructorSchedule.SevenAMStudent = st.Student;
                            instructorSchedule.DaySevenAMStudent = "Day 1";
                        }
                        else if (st.DayOne.Hour == 10)
                        {
                            instructorSchedule.TenAMStudent = st.Student;
                            instructorSchedule.DayTenAMStudent = "Day 1";
                        }
                        else if (st.DayOne.Hour == 13)
                        {
                            instructorSchedule.OnePMStudent = st.Student;
                            instructorSchedule.DayOnePMStudent = "Day 1";
                        }
                        else
                        {
                            instructorSchedule.ThreePMStudent = st.Student;
                            instructorSchedule.DayThreePMStudent = "Day 1";
                        }
                    }

                    if (st.DayTwo.Date == DateTime.Now.Date)
                    {
                        if (st.DayTwo.Hour == 7)
                        {
                            instructorSchedule.SevenAMStudent = st.Student;
                            instructorSchedule.DaySevenAMStudent = "Day 2";
                        }
                        else if (st.DayTwo.Hour == 10)
                        {
                            instructorSchedule.TenAMStudent = st.Student;
                            instructorSchedule.DayTenAMStudent = "Day 2";
                        }
                        else if (st.DayTwo.Hour == 13)
                        {
                            instructorSchedule.OnePMStudent = st.Student;
                            instructorSchedule.DayOnePMStudent = "Day 2";
                        }
                        else
                        {
                            instructorSchedule.ThreePMStudent = st.Student;
                            instructorSchedule.DayThreePMStudent = "Day 2";
                        }
                    }

                    if (st.DayThree.Date == DateTime.Now.Date)
                    {
                        if (st.DayThree.Hour == 7)
                        {
                            instructorSchedule.SevenAMStudent = st.Student;
                            instructorSchedule.DaySevenAMStudent = "Day 3";
                        }
                        else if (st.DayThree.Hour == 10)
                        {
                            instructorSchedule.TenAMStudent = st.Student;
                            instructorSchedule.DayTenAMStudent = "Day 3";
                        }
                        else if (st.DayThree.Hour == 13)
                        {
                            instructorSchedule.OnePMStudent = st.Student;
                            instructorSchedule.DayOnePMStudent = "Day 3";
                        }
                        else
                        {
                            instructorSchedule.ThreePMStudent = st.Student;
                            instructorSchedule.DayThreePMStudent = "Day 3";
                        }
                    }

                    if (st.DayFour.Date == DateTime.Now.Date)
                    {
                        if (st.DayFour.Hour == 7)
                        {
                            instructorSchedule.SevenAMStudent = st.Student;
                            instructorSchedule.DaySevenAMStudent = "Day 4";
                        }
                        else if (st.DayFour.Hour == 10)
                        {
                            instructorSchedule.TenAMStudent = st.Student;
                            instructorSchedule.DayTenAMStudent = "Day 4";
                        }
                        else if (st.DayFour.Hour == 13)
                        {
                            instructorSchedule.OnePMStudent = st.Student;
                            instructorSchedule.DayOnePMStudent = "Day 4";
                        }
                        else
                        {
                            instructorSchedule.ThreePMStudent = st.Student;
                            instructorSchedule.DayThreePMStudent = "Day 4";
                        }
                    }
                }
                ActivityLog.Create(_userManager.GetUserId(User), Area.Schedule, Models.Action.View, "Viewed schedule", _context);

                return View("InstructorSchedule", instructorSchedule);
            }
            else //student
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

                ActivityLog.Create(_userManager.GetUserId(User), Area.Schedule, Models.Action.View, "Viewed schedule", _context);
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
                .Include(s => s.Student)
                .ThenInclude(s => s.Instructor)
                .FirstOrDefaultAsync(m => m.Id == id);
            ActivityLog.Create(_userManager.GetUserId(User), Area.Schedule, Models.Action.View, "Viewed schedule", _context);

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
                ActivityLog.Create(_userManager.GetUserId(User), Area.Schedule, Models.Action.Create, "Created schedule for approval", _context);
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
                    if (schedule.Status == Status.Approved)
                    {
                        schedule.ApprovedAt = DateTime.Now;
                        var student = _context.Students.Where(x => x.Id == schedule.StudentId).FirstOrDefault();
                        if (student.EnrolledAt == null)
                        {
                            student.EnrolledAt = DateTime.Now;
                            ActivityLog.Create(_userManager.GetUserId(User), Area.Student, Models.Action.Edit, "Edited student to Enrolled", _context);
                            _context.Update(student);
                        }
                        ActivityLog.Create(_userManager.GetUserId(User), Area.Schedule, Models.Action.Edit, "Edited schedule to " + schedule.Status, _context);

                    }
                    else
                    {
                        schedule.Status = Status.Pending;
                        ActivityLog.Create(_userManager.GetUserId(User), Area.Schedule, Models.Action.Edit, "Edited Schedule", _context);
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
                    ActivityLog.Create(_userManager.GetUserId(User), Area.RequestReschedule, Models.Action.Create, "Created Request Reschedule", _context);
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
            ActivityLog.Create(_userManager.GetUserId(User), Area.Schedule, Models.Action.Delete, "Deleted Schedule", _context);
            return RedirectToAction(nameof(Index));
        }

        private bool ScheduleExists(Guid id)
        {
            return _context.Schedules.Any(e => e.Id == id);
        }
    }
}
