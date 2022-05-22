using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GTAC.Data;
using GTAC.Models;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using GTAC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace GTAC.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class ModulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ModulesController(ApplicationDbContext context,
            UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Dashboard/Modules
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            var applicationDbContext = _context.Modules.Include(m => m.Uploader);
            ActivityLog.Create(user.Id, Area.Module, Models.Action.View, "Viewed Modules", _context);

            if (roles.Count == 0)
            {
                var isEnrolled = (await _context.Students.Where(s => s.UserId == user.Id).FirstOrDefaultAsync())?.EnrolledAt != null;

                if (isEnrolled)
                {
                    return View(await applicationDbContext.ToListAsync());
                }
                else
                {
                    return View();
                }
            }

            return View(await applicationDbContext.ToListAsync());
        }

        public IActionResult ViewPDF(string filename = null, string title = null)
        {
            ActivityLog.Create(_userManager.GetUserId(User), Area.Module, Models.Action.View, "Viewed Module " + title + " PDF", _context);
            ViewData["PDFView"] = filename;
            return View();
        }

        // GET: Dashboard/Modules/Create
        [Authorize(Roles = "Instructor")]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [Authorize(Roles = "Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ModuleUpload,Title")] Module module)
        {
            if (ModelState.IsValid)
            {
                string filename = "";
                if (module.ModuleUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "modules");
                    filename = Guid.NewGuid().ToString() + "_" + module.Title + Path.GetExtension(module.ModuleUpload.FileName);
                    string filePath = Path.Combine(uploadsDir, filename);
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await module.ModuleUpload.CopyToAsync(fs);
                    fs.Close();
                }
                module.Path = filename;
                module.UploaderId = _userManager.GetUserId(User);
                module.Id = Guid.NewGuid();
                _context.Add(module);
                await _context.SaveChangesAsync();
                ActivityLog.Create(_userManager.GetUserId(User), Area.Module, Models.Action.Create, "Created a Module " + module.Title, _context);

                return RedirectToAction(nameof(Index));
            }
            return View(module);
        }

        // GET: Dashboard/Modules/Edit/5
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var module = await _context.Modules.FindAsync(id);
            if (module == null)
            {
                return NotFound();
            }
            var moduleVM = new EditModuleViewModel
            {
                Id = module.Id,
                ModuleUpload = module.ModuleUpload,
                Path = module.Path,
                Title = module.Title,
                UploaderId = module.UploaderId
            };
            return View(moduleVM);
        }

        // POST: Dashboard/Modules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Path,Title,UploaderId,ModuleUpload")] EditModuleViewModel module)
        {
            if (id != module.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string filename = "";
                    var m = new Module
                    {
                        Id = module.Id,
                        ModuleUpload = module.ModuleUpload,
                        Path = module.Path,
                        Title = module.Title,
                        UploaderId = module.UploaderId
                    };
                    if (module.ModuleUpload != null)
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "modules");
                        filename = Guid.NewGuid().ToString() + "_" + module.Title + Path.GetExtension(module.ModuleUpload.FileName);
                        string filePath = Path.Combine(uploadsDir, filename);
                        FileStream fs = new FileStream(filePath, FileMode.Create);
                        await module.ModuleUpload.CopyToAsync(fs);
                        fs.Close();
                        m.Path = filename;
                    }
                    ActivityLog.Create(_userManager.GetUserId(User), Area.Module, Models.Action.Edit, "Edited a Module " + module.Title, _context);

                    _context.Update(m);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModuleExists(module.Id))
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
            return View(module);
        }

        // GET: Dashboard/Modules/Delete/5
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var module = await _context.Modules
                .Include(m => m.Uploader)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (module == null)
            {
                return NotFound();
            }

            return View(module);
        }

        // POST: Dashboard/Modules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var module = await _context.Modules.FindAsync(id);
            _context.Modules.Remove(module);
            ActivityLog.Create(_userManager.GetUserId(User), Area.Module, Models.Action.Edit, "Deleted a Module " + module.Title, _context);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ModuleExists(Guid id)
        {
            return _context.Modules.Any(e => e.Id == id);
        }
    }
}
