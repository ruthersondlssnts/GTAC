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
using GTAC.Models.ViewModels;
using System.IO;

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
        public async Task<IActionResult> Index(string toEnroll = null, string fromEnroll = null, string fromGrad = null, string toGrad = null)
        {
            ActivityLog.Create(_userManager.GetUserId(User), Area.Student, Models.Action.View, "Viewed Students", _context);
            ViewBag.toEnroll = toEnroll ?? null;
            ViewBag.fromEnroll = fromEnroll ?? null;
            ViewBag.fromGrad = fromGrad ?? null;
            ViewBag.toGrad = toGrad ?? null;

            if (User.IsInRole("Instructor"))
            {
                var applicationDbContext = _context.Students.Include(s => s.User).Where(s => s.InstructorId == _userManager.GetUserId(User));
                return View(await applicationDbContext.ToListAsync());
            }
            else
            {
                var applicationDbContext = await _context.Students
                    .Include(s => s.Instructor)
                    .Include(s => s.User)
                    .Where(a => toEnroll != null ? a.EnrolledAt != null && a.EnrolledAt.Value.Date <= DateTime.Parse(toEnroll).Date : true)
                    .Where(a => fromEnroll != null ? a.EnrolledAt != null && a.EnrolledAt.Value.Date >= DateTime.Parse(fromEnroll).Date : true)
                    .Where(a => fromGrad != null ? a.GraduatedAt != null && a.GraduatedAt.Value.Date >= DateTime.Parse(fromGrad).Date : true)
                    .Where(a => toGrad != null ? a.GraduatedAt != null && a.GraduatedAt.Value.Date <= DateTime.Parse(toGrad).Date : true)
                    .ToListAsync();
                return View(applicationDbContext);
            }

        }

        // GET: Dashboard/Students
        public async Task<IActionResult> ViewProfile()
        {
            string userId = _userManager.GetUserId(User);
            ActivityLog.Create(userId, Area.Student, Models.Action.View, "Viewed Profile", _context);

            var student = await _context.Students.Include(s => s.User).Include(s => s.Instructor).Where(s => s.UserId == userId).FirstOrDefaultAsync();

            return View(student);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatusChangeRequest(Guid id, Status status)
        {
            var request = await _context.StudentChangeRequests.FindAsync(id);
            var user = await _context.Users.FindAsync(request.UserId);

            if (status == Status.Approved)
            {
                if (request.PhotoPath != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images/users");
                    string filePath = Path.Combine(uploadsDir, user.PhotoPath);
                    if ((System.IO.File.Exists(filePath)))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    user.PhotoPath = request.PhotoPath;
                }
                user.Firstname = request.Firstname;
                user.Lastname = request.Lastname;
                user.Middlename = request.Middlename;
                user.Suffix = request.Suffix;
                user.Address = request.Address;
                user.Birthday = Convert.ToDateTime(request.Birthday);
                user.PhoneNumber = request.PhoneNumber;
                user.IsProfileApproved = true;
                IdentityResult result = await _userManager.UpdateAsync(user);

                _context.Remove(request);


                await _context.SaveChangesAsync();
            }
            else if (status == Status.Reject)
            {
                user.IsProfileApproved = true;
                IdentityResult result = await _userManager.UpdateAsync(user);
                if (request.PhotoPath != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images/users");
                    string filePath = Path.Combine(uploadsDir, request.PhotoPath);
                    if ((System.IO.File.Exists(filePath)))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                _context.Remove(request);
                await _context.SaveChangesAsync();
            }


            return RedirectToAction("Details", new { id = (await _context.Students.Where(s => s.UserId == user.Id).FirstOrDefaultAsync()).Id });
        }

        // GET: Dashboard/Students
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            EditUserViewModel uvm = new EditUserViewModel
            {
                Email = user.Email,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Middlename = user.Middlename,
                Suffix = user.Suffix,
                Address = user.Address,
                Birthday = user.Birthday.ToString(),
                PhoneNumber = user.PhoneNumber,
                PhotoPath = user.PhotoPath,
                Id = user.Id
            };

            return View(uvm);
        }

        // POST: UsersController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditUserViewModel uvm)
        {
            try
            {
                StudentChangeRequest studentChangeRequest = new StudentChangeRequest();
                studentChangeRequest.Id = Guid.NewGuid();
                studentChangeRequest.Firstname = uvm.Firstname;
                studentChangeRequest.Lastname = uvm.Lastname;
                studentChangeRequest.Middlename = uvm.Middlename;
                studentChangeRequest.Suffix = uvm.Suffix;
                studentChangeRequest.Address = uvm.Address;
                studentChangeRequest.Birthday = Convert.ToDateTime(uvm.Birthday);
                studentChangeRequest.PhoneNumber = uvm.PhoneNumber;
                studentChangeRequest.UserId = uvm.Id;

                if (uvm.Photo != null)
                {
                    string filename = "";
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images/users");
                    filename = studentChangeRequest.Id + Path.GetExtension(uvm.Photo.FileName);
                    string filePath = Path.Combine(uploadsDir, filename);
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await uvm.Photo.CopyToAsync(fs);
                    fs.Close();

                    studentChangeRequest.PhotoPath = filename;
                }


                _context.Add(studentChangeRequest);
                var user = await _userManager.GetUserAsync(User);
                user.IsProfileApproved = false;
                IdentityResult result = await _userManager.UpdateAsync(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ViewProfile));
                //IdentityResult result = await _userManager.UpdateAsync(user);

                //if (result.Succeeded)
                //{
                //    ActivityLog.Create(_userManager.GetUserId(User), Area.CompanyUser, Models.Action.Edit, "Edited a student profile " + user.Firstname + " " + user.Lastname, _context);
                //    
                //}
                //else
                //{
                //    foreach (IdentityError error in result.Errors)
                //    {
                //        ModelState.AddModelError("", error.Description);
                //    }
                //}
                //return View(user);
            }
            catch
            {
                return View();
            }
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
            ViewBag.ChangeRequest = null;

            if (student.User.IsProfileApproved == false)
            {
                ViewBag.ChangeRequest = await _context.StudentChangeRequests.Where(s => s.UserId == student.UserId).FirstOrDefaultAsync();
            }
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteChangeRequest(Guid id)
        {
            var requestReprofile = await _context.StudentChangeRequests.FindAsync(id);
            _context.StudentChangeRequests.Remove(requestReprofile);
            var sched = await _context.Users.FindAsync(requestReprofile.UserId);
            sched.IsProfileApproved = true;
            if (requestReprofile.PhotoPath != null)
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images/users");
                string filePath = Path.Combine(uploadsDir, requestReprofile.PhotoPath);
                if ((System.IO.File.Exists(filePath)))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            await _context.SaveChangesAsync();
            // ActivityLog.Create(_userManager.GetUserId(User), Area., Models.Action.Delete, "Deleted Request for Reschedule", _context);
            return RedirectToAction(nameof(ViewProfile));
        }

        private bool StudentExists(Guid id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
