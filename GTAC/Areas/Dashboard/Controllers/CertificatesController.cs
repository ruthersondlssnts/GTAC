﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GTAC.Data;
using GTAC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace GTAC.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class CertificatesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CertificatesController(ApplicationDbContext context,
            UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Dashboard/Certificates
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var applicationDbContext = await _context.Certificates.Include(c => c.Approver).Include(c => c.Author).FirstOrDefaultAsync();
            var roles = await _userManager.GetRolesAsync(user);
            if (applicationDbContext == null && roles.Count > 0)
            {
                return View(nameof(Create));
            }
            ViewBag.isStudent = roles.Count == 0;
            ViewBag.isGraduated = roles.Count == 0 ? (await _context.Students.Where(s => s.UserId == user.Id).FirstOrDefaultAsync())?.GraduatedAt != null : true;
            return View("Details", applicationDbContext);
        }

        // GET: Dashboard/Certificates/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificate = await _context.Certificates
                .Include(c => c.Approver)
                .Include(c => c.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (certificate == null)
            {
                return NotFound();
            }

            return View(certificate);
        }

        // GET: Dashboard/Certificates/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Dashboard/Certificates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Path,CertificateUpload")] Certificate certificate)
        {
            if (ModelState.IsValid)
            {
                certificate.Id = Guid.NewGuid();
                string filename = "";
                if (certificate.CertificateUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    filename = Guid.NewGuid().ToString() + Path.GetExtension(certificate.CertificateUpload.FileName);
                    string filePath = Path.Combine(uploadsDir, filename);
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await certificate.CertificateUpload.CopyToAsync(fs);
                    fs.Close();
                }
                certificate.Path = filename;
                certificate.AuthorId = _userManager.GetUserId(User);
                certificate.Status = Status.Pending;
                _context.Add(certificate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(certificate);
        }

        // GET: Dashboard/Certificates/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate == null)
            {
                return NotFound();
            }
            return View(certificate);
        }

        // POST: Dashboard/Certificates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Path,AuthorId,ApproverId,ApprovedDate,Status,CertificateUpload")] Certificate certificate)
        {
            ModelState.Remove("CertificateUpload");
            if (id != certificate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var oldcertificate = await _context.Certificates.FindAsync(id);
                    if (certificate.CertificateUpload != null)
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        string filePath = Path.Combine(uploadsDir, certificate.Path);
                        FileStream fs = new FileStream(filePath, FileMode.Create);
                        await certificate.CertificateUpload.CopyToAsync(fs);
                        fs.Close();
                        oldcertificate.Path = certificate.Path;
                        oldcertificate.Status = Status.Pending;
                        oldcertificate.ApproverId = null;
                        oldcertificate.ApprovedDate = null;
                    }
                    else if (oldcertificate.Status != Status.Approved && certificate.Status == Status.Approved)
                    {
                        oldcertificate.ApprovedDate = DateTime.Now;
                        oldcertificate.ApproverId = _userManager.GetUserId(User);
                        oldcertificate.Status = Status.Approved;
                    }
                    _context.Update(oldcertificate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CertificateExists(certificate.Id))
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
            return View(certificate);
        }

        // GET: Dashboard/Certificates/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificate = await _context.Certificates
                .Include(c => c.Approver)
                .Include(c => c.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (certificate == null)
            {
                return NotFound();
            }

            return View(certificate);
        }

        // POST: Dashboard/Certificates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var certificate = await _context.Certificates.FindAsync(id);
            _context.Certificates.Remove(certificate);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CertificateExists(Guid id)
        {
            return _context.Certificates.Any(e => e.Id == id);
        }
    }
}
