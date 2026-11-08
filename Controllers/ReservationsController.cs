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
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Reservations.Include(r => r.Organization).Include(r => r.Venue);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Organization)
                .Include(r => r.Venue)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservations/Create
        public IActionResult Create()
        {
            ViewData["OrganizationId"] = new SelectList(_context.Organization, "Id", "Name");
            ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name");
            return View();
        }

        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrganizationId,VenueId,ActivityName,ReservationDate,StartTime,EndTime,ExpectedAttendees,RequesterName,RequesterEmail,RequesterContact")] Reservation reservation)
        {
            // 1. The date cannot be in the past
            if (reservation.ReservationDate < DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError("ReservationDate", "You cannot reserve a date that has already passed.");
            }

            // 2. End time must be after start time
            if (reservation.EndTime <= reservation.StartTime)
            {
                ModelState.AddModelError("EndTime", "End time must be later than the start time.");
            }

            // 3. Reservations are only from 7:30 AM to 5:00 PM
            TimeOnly openingTime = new TimeOnly(7, 30);
            TimeOnly closingTime = new TimeOnly(17, 0);

            if (reservation.StartTime < openingTime || reservation.EndTime > closingTime)
            {
                ModelState.AddModelError("StartTime", "Reservations are only allowed from 7:30 AM to 5:00 PM.");
            }

            // 4. The venue must be able to hold the expected number of people
            var venue = await _context.Venue.FindAsync(reservation.VenueId);

            if (venue != null && reservation.ExpectedAttendees > venue.Capacity)
            {
                ModelState.AddModelError("ExpectedAttendees",
                    "This venue can only hold " + venue.Capacity + " people.");
            }

            // 5. The venue must not already be booked at an overlapping time
            bool conflict = await _context.Reservations.AnyAsync(r =>
                r.VenueId == reservation.VenueId &&
                r.ReservationDate == reservation.ReservationDate &&
                r.Status != "Rejected" &&
                r.StartTime < reservation.EndTime &&
                reservation.StartTime < r.EndTime);

            if (conflict)
            {
                ModelState.AddModelError("StartTime",
                    "This venue is already reserved during that time. Please choose another slot.");
            }

            if (ModelState.IsValid)
            {
                // Generate the tracking number
                int count = await _context.Reservations.CountAsync() + 1;
                reservation.TrackingNumber = "VRS-" + DateTime.Now.Year + "-" + count.ToString("D4");

                reservation.Status = "Pending";
                reservation.CurrentStep = 1;
                reservation.DateSubmitted = DateTime.Now;

                _context.Add(reservation);
                await _context.SaveChangesAsync();

                TempData["Tracking"] = reservation.TrackingNumber;
                return RedirectToAction(nameof(Index));
            }

            ViewData["OrganizationId"] = new SelectList(_context.Organization, "Id", "Name", reservation.OrganizationId);
            ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name", reservation.VenueId);
            return View(reservation);
        }

        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            ViewData["OrganizationId"] = new SelectList(_context.Organization, "Id", "Name", reservation.OrganizationId);
            ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name", reservation.VenueId);
            return View(reservation);
        }

        // POST: Reservations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TrackingNumber,OrganizationId,VenueId,ActivityName,ReservationDate,StartTime,EndTime,ExpectedAttendees,RequesterName,RequesterEmail,RequesterContact,Status,CurrentStep,DateSubmitted")] Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.Id))
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
            ViewData["OrganizationId"] = new SelectList(_context.Organization, "Id", "Name", reservation.OrganizationId);
            ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name", reservation.VenueId);
            return View(reservation);
        }

        // GET: Reservations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Organization)
                .Include(r => r.Venue)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Reservations/Track
        public async Task<IActionResult> Track(string? trackingNumber)
        {
            if (string.IsNullOrEmpty(trackingNumber))
            {
                return View(null);
            }

            var reservation = await _context.Reservations
                .Include(r => r.Organization)
                .Include(r => r.Venue)
                .FirstOrDefaultAsync(r => r.TrackingNumber == trackingNumber);

            if (reservation == null)
            {
                ViewBag.NotFound = "No reservation found with tracking number " + trackingNumber + ".";
                return View(null);
            }

            return View(reservation);
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
}