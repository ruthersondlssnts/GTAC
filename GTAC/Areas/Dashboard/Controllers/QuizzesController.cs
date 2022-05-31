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
    public class QuizzesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public QuizzesController(ApplicationDbContext context,
           UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> UpdateStudentQuiz(Guid Id, bool isMarkDone = false)
        {
            var studentId = _context.Students.Where(s => s.UserId == _userManager.GetUserId(User)).FirstOrDefault().Id;

            if (isMarkDone)
            {
                var quizStudent = new Quiz_Student { Id = new Guid(), QuizId = Id, StudentId = studentId };
                _context.Add(quizStudent);
                ActivityLog.Create(_userManager.GetUserId(User), Area.Quiz, Models.Action.Edit, "Edited a Quiz status to Done", _context);
                await _context.SaveChangesAsync();
            }
            else
            {
                var quiz_stud = await _context.Quiz_Students.Where(q => q.QuizId == Id && q.StudentId == studentId).FirstOrDefaultAsync();
                ActivityLog.Create(_userManager.GetUserId(User), Area.Quiz, Models.Action.Edit, "Edited a Quiz status to Unsubmitted", _context);
                _context.Quiz_Students.Remove(quiz_stud);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Dashboard/Quizzes
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            ActivityLog.Create(_userManager.GetUserId(User), Area.Quiz, Models.Action.View, "View Quizzes", _context);

            if (roles.Count == 0)
            {
                var isEnrolled = (await _context.Students.Where(s => s.UserId == user.Id).FirstOrDefaultAsync())?.EnrolledAt != null;

                if (isEnrolled)
                {
                    var studentId = _context.Students.Where(s => s.UserId == user.Id).FirstOrDefault().Id;
                    var quizzes = (from quiz in _context.Quizzes
                                   select new StudentQuizzesViewModel { Id = quiz.Id, Name = quiz.Name, Link = quiz.Link, isDone = _context.Quiz_Students.Any(q => q.QuizId == quiz.Id && q.StudentId == studentId) ? true : false }).ToList();
                    return View(quizzes);
                }
                return View();
            }
            var applicationDbContext = await _context.Quizzes.Include(q => q.Author).ToListAsync();
            return View(applicationDbContext.Select(s => new StudentQuizzesViewModel { Id = s.Id, Link = s.Link, Name = s.Name, Author = s.Author, isAnyStudents = _context.Quiz_Students.Any(x => x.QuizId == s.Id) }));
        }

        // GET: Dashboard/Quizzes/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quizzes
                .Include(q => q.Author)
                .FirstOrDefaultAsync(m => m.Id == id);


            if (quiz == null)
            {
                return NotFound();
            }

            ActivityLog.Create(_userManager.GetUserId(User), Area.Quiz, Models.Action.View, "Viewed a Quiz and finished students", _context);

            ViewBag.Students = await _context.Quiz_Students
                .Include(qs => qs.Student)
                .ThenInclude(s => s.User)
                .Include(qs => qs.Student)
                .ThenInclude(s => s.Instructor)
                .Where(qs => qs.QuizId == id).Select(qs => qs.Student)
                .ToListAsync();
            return View(quiz);
        }

        // GET: Dashboard/Quizzes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Dashboard/Quizzes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Link,Name")] Quiz quiz)
        {
            if (ModelState.IsValid)
            {
                quiz.Id = Guid.NewGuid();
                quiz.AuthorId = _userManager.GetUserId(User);
                _context.Add(quiz);
                ActivityLog.Create(_userManager.GetUserId(User), Area.Quiz, Models.Action.Create, "Created a Quiz " + quiz.Name, _context);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(quiz);
        }

        // GET: Dashboard/Quizzes/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }
            return View(quiz);
        }

        // POST: Dashboard/Quizzes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Link,Name,AuthorId")] Quiz quiz)
        {
            if (id != quiz.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quiz);
                    ActivityLog.Create(_userManager.GetUserId(User), Area.Quiz, Models.Action.Edit, "Edited a Quiz " + quiz.Name, _context);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuizExists(quiz.Id))
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
            return View(quiz);
        }

        // GET: Dashboard/Quizzes/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quizzes
                .Include(q => q.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        // POST: Dashboard/Quizzes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            _context.Quizzes.Remove(quiz);
            ActivityLog.Create(_userManager.GetUserId(User), Area.Quiz, Models.Action.Delete, "Delete a Quiz " + quiz.Name, _context);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuizExists(Guid id)
        {
            return _context.Quizzes.Any(e => e.Id == id);
        }
    }
}
