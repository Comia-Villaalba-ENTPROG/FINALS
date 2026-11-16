using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FINALS.Data;
using FINALS.Models;
using Microsoft.AspNetCore.Authorization;

namespace FINALS.Controllers
{
    [Authorize]
    public class SchoolClosuresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SchoolClosuresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SchoolClosures
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _context.SchoolClosures.ToListAsync());
        }

        // GET: SchoolClosures/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolClosure = await _context.SchoolClosures
                .FirstOrDefaultAsync(m => m.Id == id);
            if (schoolClosure == null)
            {
                return NotFound();
            }

            return View(schoolClosure);
        }

        // GET: SchoolClosures/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SchoolClosures/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StartDate,EndDate,Reason")] SchoolClosure schoolClosure)
        {
            if (ModelState.IsValid)
            {
                _context.Add(schoolClosure);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(schoolClosure);
        }

        // GET: SchoolClosures/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolClosure = await _context.SchoolClosures.FindAsync(id);
            if (schoolClosure == null)
            {
                return NotFound();
            }
            return View(schoolClosure);
        }

        // POST: SchoolClosures/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StartDate,EndDate,Reason")] SchoolClosure schoolClosure)
        {
            if (id != schoolClosure.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(schoolClosure);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SchoolClosureExists(schoolClosure.Id))
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
            return View(schoolClosure);
        }

        // GET: SchoolClosures/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolClosure = await _context.SchoolClosures
                .FirstOrDefaultAsync(m => m.Id == id);
            if (schoolClosure == null)
            {
                return NotFound();
            }

            return View(schoolClosure);
        }

        // POST: SchoolClosures/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schoolClosure = await _context.SchoolClosures.FindAsync(id);
            if (schoolClosure != null)
            {
                _context.SchoolClosures.Remove(schoolClosure);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SchoolClosureExists(int id)
        {
            return _context.SchoolClosures.Any(e => e.Id == id);
        }
    }
}
