using System;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _http;

        public ReservationsController(ApplicationDbContext context, IHttpClientFactory httpFactory)
        {
            _context = context;
            _http = httpFactory.CreateClient();
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

            ViewBag.Equipment = await _context.ReservationEquipments
                .Include(re => re.Equipment)
                .Where(re => re.ReservationId == reservation.Id)
                .ToListAsync();

            return View(reservation);
        }

        // GET: Reservations/Create
        [AllowAnonymous]
        public IActionResult Create()
        {
            ViewData["OrganizationId"] = new SelectList(_context.Organization, "Id", "Name");
            ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name");
            ViewBag.EquipmentList = _context.Equipment.OrderBy(e => e.Name).ToList();
            return View();
        }

        // POST: Reservations/Create
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("OrganizationId,VenueId,ActivityName,ReservationDate,StartTime,EndTime,ExpectedAttendees,RequesterName,RequesterEmail,RequesterContact")] Reservation reservation,
            int[] equipmentIds,
            int[] quantities)
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

            // 6. Enough equipment must still be free on that date
            for (int i = 0; i < equipmentIds.Length; i++)
            {
                if (quantities[i] <= 0)
                {
                    continue;
                }

                var item = await _context.Equipment.FindAsync(equipmentIds[i]);

                if (item == null)
                {
                    continue;
                }

                int equipmentId = equipmentIds[i];

                int alreadyUsed = await _context.ReservationEquipments
                    .Where(re => re.EquipmentId == equipmentId
                              && re.Reservation!.ReservationDate == reservation.ReservationDate
                              && re.Reservation.Status != "Rejected")
                    .SumAsync(re => (int?)re.Quantity) ?? 0;

                int stillFree = item.Quantity - alreadyUsed;

                if (quantities[i] > stillFree)
                {
                    ModelState.AddModelError("",
                        "Only " + stillFree + " " + item.Name + " are available on that date. You asked for " + quantities[i] + ".");
                }
            }

             try
            {
                int year = reservation.ReservationDate.Year;
                var url = "https://date.nager.at/api/v3/PublicHolidays/" + year + "/PH";

                var holidays = await _http.GetFromJsonAsync<List<Holiday>>(url) ?? new List<Holiday>();

                foreach (var holiday in holidays)
                {
                    if (holiday.Date == reservation.ReservationDate)
                    {
                        ModelState.AddModelError("ReservationDate",
                            "The school is closed on " + holiday.Date.ToString("MMMM d") +
                            ", " + holiday.LocalName + " (" + holiday.Name + "). Please choose another date.");
                        break;
                    }
                }
            }
            catch (Exception)
            {
                
            }

            if (ModelState.IsValid)
            {
                // Generate the tracking number
                int count = await _context.Reservations.CountAsync() + 1;
                reservation.TrackingNumber = "VRS-" + DateTime.Now.Year + "-" + count.ToString("D4");

                reservation.ApprovalToken = Guid.NewGuid().ToString("N");

                reservation.Status = "Pending";
                reservation.CurrentStep = 1;
                reservation.DateSubmitted = DateTime.Now;

                _context.Add(reservation);
                await _context.SaveChangesAsync();

                // Save the equipment lines now that the reservation has an Id
                for (int i = 0; i < equipmentIds.Length; i++)
                {
                    if (quantities[i] > 0)
                    {
                        var line = new ReservationEquipment();
                        line.ReservationId = reservation.Id;
                        line.EquipmentId = equipmentIds[i];
                        line.Quantity = quantities[i];

                        _context.Add(line);
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Track", new { trackingNumber = reservation.TrackingNumber });
            }

            ViewData["OrganizationId"] = new SelectList(_context.Organization, "Id", "Name", reservation.OrganizationId);
            ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name", reservation.VenueId);
            ViewBag.EquipmentList = _context.Equipment.OrderBy(e => e.Name).ToList();
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

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }

        // GET: Reservations/Track
        [AllowAnonymous]
        public async Task<IActionResult> Track(string? trackingNumber)
        {
            if (string.IsNullOrEmpty(trackingNumber))
            {
                return View((Reservation?)null);
            }

            var reservation = await _context.Reservations
                .Include(r => r.Organization)
                .Include(r => r.Venue)
                .FirstOrDefaultAsync(r => r.TrackingNumber == trackingNumber);

            if (reservation == null)
            {
                ViewBag.NotFound = "No reservation found with tracking number " + trackingNumber + ".";
                return View((Reservation?)null);
            }

            ViewBag.Equipment = await _context.ReservationEquipments
                .Include(re => re.Equipment)
                .Where(re => re.ReservationId == reservation.Id)
                .ToListAsync();

            return View(reservation);
        }

        // GET: Reservations/Approvals
        
        public async Task<IActionResult> Approvals()
        {
            var pending = await _context.Reservations
                .Include(r => r.Organization)
                .Include(r => r.Venue)
                .Where(r => r.Status == "Pending")
                .OrderBy(r => r.ReservationDate)
                .ToListAsync();

            // Work out which office each reservation is waiting on
            var offices = new Dictionary<int, string>();

            foreach (var r in pending)
            {
                var approver = await _context.VenueApprover
                    .FirstOrDefaultAsync(a => a.VenueId == r.VenueId && a.StepOrder == r.CurrentStep);

                offices[r.Id] = approver != null ? approver.Office : "No approver set";
            }

            ViewBag.Offices = offices;
            return View(pending);
        }

        // POST: Reservations/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return NotFound();
            }

            await AdvanceApproval(reservation);

            TempData["Message"] = reservation.Status == "Approved"
                ? reservation.TrackingNumber + " is now fully approved."
                : reservation.TrackingNumber + " moved to approval step " + reservation.CurrentStep + ".";

            return RedirectToAction(nameof(Approvals));
        }

        // POST: Reservations/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return NotFound();
            }

            reservation.Status = "Rejected";
            reservation.ApprovalToken = "";
            await _context.SaveChangesAsync();

            TempData["Message"] = reservation.TrackingNumber + " has been rejected.";
            return RedirectToAction(nameof(Approvals));
        }

        // Shared helper so the logged-in and link approvals behave identically
        private async Task AdvanceApproval(Reservation reservation)
        {
            int lastStep = await _context.VenueApprover
                .Where(a => a.VenueId == reservation.VenueId)
                .MaxAsync(a => (int?)a.StepOrder) ?? 1;

            if (reservation.CurrentStep >= lastStep)
            {
                reservation.Status = "Approved";
                reservation.ApprovalToken = "";          // no more links needed
            }
            else
            {
                reservation.CurrentStep = reservation.CurrentStep + 1;
                reservation.ApprovalToken = Guid.NewGuid().ToString("N");   // fresh link for the next office
            }

            await _context.SaveChangesAsync();
        }

        // GET: Reservations/ExternalApprove?token=xxxx
        [AllowAnonymous]
        public async Task<IActionResult> ExternalApprove(string token)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Organization)
                .Include(r => r.Venue)
                .FirstOrDefaultAsync(r => r.ApprovalToken == token && r.Status == "Pending");

            if (reservation == null)
            {
                ViewBag.Invalid = "This approval link is no longer valid. It may have already been used, or the reservation has been decided.";
                return View((Reservation?)null);
            }

            var approver = await _context.VenueApprover
                .FirstOrDefaultAsync(a => a.VenueId == reservation.VenueId && a.StepOrder == reservation.CurrentStep);

            ViewBag.Office = approver != null ? approver.Office : "the assigned office";
            ViewBag.Token = token;

            ViewBag.Equipment = await _context.ReservationEquipments
                .Include(re => re.Equipment)
                .Where(re => re.ReservationId == reservation.Id)
                .ToListAsync();

            return View(reservation);
        }

        // POST: the office clicks Approve or Reject on that page
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalDecision(string token, string decision)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Organization)
                .Include(r => r.Venue)
                .FirstOrDefaultAsync(r => r.ApprovalToken == token && r.Status == "Pending");

            if (reservation == null)
            {
                ViewBag.Invalid = "This approval link is no longer valid.";
                return View("ExternalApprove", (Reservation?)null);
            }

            if (decision == "Reject")
            {
                reservation.Status = "Rejected";
                reservation.ApprovalToken = "";
                await _context.SaveChangesAsync();
            }
            else
            {
                await AdvanceApproval(reservation);
            }

            return View("ExternalDone", reservation);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Availability(DateOnly date)
        {
            var equipment = await _context.Equipment.ToListAsync();
            var result = new List<object>();

            foreach (var item in equipment)
            {
                int itemId = item.Id;

                int alreadyUsed = await _context.ReservationEquipments
                    .Where(re => re.EquipmentId == itemId
                              && re.Reservation!.ReservationDate == date
                              && re.Reservation.Status != "Rejected")
                    .SumAsync(re => (int?)re.Quantity) ?? 0;

                result.Add(new { id = item.Id, free = item.Quantity - alreadyUsed });
            }

            return Json(result);
        }
    }
}