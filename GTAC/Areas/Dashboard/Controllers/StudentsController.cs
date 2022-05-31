using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GTAC.Data;
using GTAC.Models;
using Microsoft.AspNetCore.Hosting;
using AspNetCore.Reporting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace GTAC.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<User> _userManager;

        public StudentsController(ApplicationDbContext context, UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }
        // GET: Dashboard/Students
        public IActionResult Print()
        {
            ActivityLog.Create(_userManager.GetUserId(User), Area.Student, Models.Action.View, "Viewed graduated students report", _context);

            var students = _context.Students
                .Include(s => s.Instructor)
                .Include(s => s.User)
                .Where(s => s.GraduatedAt != null)
                .Select(s => new
                {
                    Firstname = s.User.Firstname,
                    Lastname = s.User.Lastname,
                    Middlename = s.User.Middlename,
                    Instructor = s.Instructor.Firstname + " " + s.Instructor.Lastname,
                    EnrolledAt = s.EnrolledAt,
                    GraduatedAt = s.GraduatedAt,
                    Email = s.User.Email,
                    Contact = s.User.PhoneNumber
                })
                .ToList();

            string minType = "";
            int extension = 1;
            var path = $"{this._webHostEnvironment.WebRootPath}\\Reports\\StudentsReport.rdlc";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            LocalReport localReport = new LocalReport(path);
            localReport.AddDataSource("dsStudents", students);
            var result = localReport.Execute(RenderType.Pdf, extension, parameters, minType);
            return File(result.MainStream, "application/pdf");
        }

        // GET: Dashboard/Students
        public async Task<IActionResult> Index()
        {
            ActivityLog.Create(_userManager.GetUserId(User), Area.Student, Models.Action.View, "Viewed Students", _context);

            var applicationDbContext = _context.Students.Include(s => s.Instructor).Include(s => s.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Dashboard/Students/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Instructor)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            ActivityLog.Create(_userManager.GetUserId(User), Area.Student, Models.Action.View, "Viewed a student", _context);

            if (student == null)
            {
                return NotFound();
            }

            ViewBag.StudSched = await _context.Schedules.Where(s => s.StudentId == id).FirstOrDefaultAsync();
            return View(student);
        }

        // GET: Dashboard/Students/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Dashboard/Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EnrolledAt,GraduatedAt,UserId,InstructorId")] Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ActivityLog.Create(_userManager.GetUserId(User), Area.Student, Models.Action.Edit, "Edited a student", _context);

                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
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

            return View(student);
        }

        // GET: Dashboard/Students/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Instructor)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Dashboard/Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {

            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            var user = await _context.Users.FindAsync(student.User.Id);
            _context.Schedules.RemoveRange(_context.Schedules.Where(s => s.StudentId == id));
            _context.ActivityLogs.RemoveRange(_context.ActivityLogs.Where(al => al.UserId == user.Id));
            _context.Students.Remove(student);
            _context.Users.Remove(user);
            ActivityLog.Create(_userManager.GetUserId(User), Area.Student, Models.Action.Delete, "Deleted a student", _context);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(Guid id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
