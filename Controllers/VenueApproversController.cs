using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FINALS.Data;
using FINALS.Models;

namespace FINALS.Controllers
{
    public class VenueApproversController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VenueApproversController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: VenueApprovers
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.VenueApprover.Include(v => v.Venue);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: VenueApprovers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venueApprover = await _context.VenueApprover
                .Include(v => v.Venue)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (venueApprover == null)
            {
                return NotFound();
            }

            return View(venueApprover);
        }

        // GET: VenueApprovers/Create
        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Id");
            return View();
        }

        // POST: VenueApprovers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VenueId,Office,Email,StepOrder")] VenueApprover venueApprover)
        {
            if (ModelState.IsValid)
            {
                _context.Add(venueApprover);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Id", venueApprover.VenueId);
            return View(venueApprover);
        }

        // GET: VenueApprovers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venueApprover = await _context.VenueApprover.FindAsync(id);
            if (venueApprover == null)
            {
                return NotFound();
            }
            ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Id", venueApprover.VenueId);
            return View(venueApprover);
        }

        // POST: VenueApprovers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VenueId,Office,Email,StepOrder")] VenueApprover venueApprover)
        {
            if (id != venueApprover.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(venueApprover);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueApproverExists(venueApprover.Id))
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
            ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Id", venueApprover.VenueId);
            return View(venueApprover);
        }

        // GET: VenueApprovers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venueApprover = await _context.VenueApprover
                .Include(v => v.Venue)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (venueApprover == null)
            {
                return NotFound();
            }

            return View(venueApprover);
        }

        // POST: VenueApprovers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venueApprover = await _context.VenueApprover.FindAsync(id);
            if (venueApprover != null)
            {
                _context.VenueApprover.Remove(venueApprover);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VenueApproverExists(int id)
        {
            return _context.VenueApprover.Any(e => e.Id == id);
        }
    }
}
