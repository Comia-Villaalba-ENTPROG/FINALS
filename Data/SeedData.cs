using System.Globalization;
using FINALS.Models;

namespace FINALS.Data;

public static class SeedData
{
    public static void Initialize(ApplicationDbContext context)
    {
        // ---------- Organizations ----------
        if (!context.Organization.Any())
        {
            string[] orgs = {
                "BASG", "BAJMA", "JASIS", "HMSOC", "JPIA",
                "EDUCERE", "COMMGUILD", "PSYCHSOC", "TPP", "BEVO",
                "LEAD", "TBS", "DWBA", "A'BLAZE", "ANIMO LUCERE"
            };

            foreach (string name in orgs)
            {
                var org = new Organization();
                org.Name = name;
                org.Chairperson = "TBA";
                context.Organization.Add(org);
            }

            context.SaveChanges();
        }

        // ---------- Venues ----------
        if (!context.Venue.Any())
        {
            context.Venue.Add(new Venue { Name = "Benilde Hall", Location = "Main Building", Capacity = 200 });
            context.Venue.Add(new Venue { Name = "Chapel", Location = "Main Building", Capacity = 50 });
            context.Venue.Add(new Venue { Name = "Lobby", Location = "Ground Floor", Capacity = 250 });
            context.Venue.Add(new Venue { Name = "Function Hall", Location = "HM Area", Capacity = 70 });
            context.Venue.Add(new Venue { Name = "GYM A", Location = "Sports Complex", Capacity = 50 });
            context.Venue.Add(new Venue { Name = "GYM B", Location = "Sports Complex", Capacity = 50 });
            context.Venue.Add(new Venue { Name = "GYM C", Location = "Sports Complex", Capacity = 400 });

            context.SaveChanges();
        }

        // ---------- Equipment ----------
        if (!context.Equipment.Any())
        {
            context.Equipment.Add(new Equipment { Name = "Monoblock Chairs", Quantity = 500 });
            context.Equipment.Add(new Equipment { Name = "Tables", Quantity = 15 });
            context.Equipment.Add(new Equipment { Name = "Stage Platforms", Quantity = 10 });
            context.Equipment.Add(new Equipment { Name = "Plants", Quantity = 4 });
            context.Equipment.Add(new Equipment { Name = "Flag Poles", Quantity = 2 });
            context.Equipment.Add(new Equipment { Name = "Podium", Quantity = 1 });

            context.SaveChanges();
        }

        // ---------- Venue Approvers ----------
        if (!context.VenueApprover.Any())
        {
            AddApprover(context, "Benilde Hall", "GASO", "benildeantipolovenuesystem+gaso@gmail.com", 1);

            AddApprover(context, "Chapel", "OLM", "benildeantipolovenuesystem+olm@gmail.com", 1);
            AddApprover(context, "Chapel", "LMSL", "benildeantipolovenuesystem+lmsl@gmail.com", 2);
            AddApprover(context, "Chapel", "GASO", "benildeantipolovenuesystem+gaso@gmail.com", 3);

            AddApprover(context, "Lobby", "GASO", "benildeantipolovenuesystem+gaso@gmail.com", 1);

            AddApprover(context, "Function Hall", "HM", "benildeantipolovenuesystem+hm@gmail.com", 1);
            AddApprover(context, "Function Hall", "GASO", "benildeantipolovenuesystem+gaso@gmail.com", 2);

            AddApprover(context, "GYM A", "GASO", "benildeantipolovenuesystem+gaso@gmail.com", 1);
            AddApprover(context, "GYM B", "GASO", "benildeantipolovenuesystem+gaso@gmail.com", 1);
            AddApprover(context, "GYM C", "GASO", "benildeantipolovenuesystem+gaso@gmail.com", 1);

            context.SaveChanges();
        }

        // ---------- School Calendar, AY 2026-2027 ----------
        if (!context.SchoolClosures.Any())
        {
            // Holidays the national API does not return
            AddClosure(context, "2026-11-02", "2026-11-02", "All Souls' Day");
            AddClosure(context, "2027-02-25", "2027-02-25", "People Power Anniversary");
            AddClosure(context, "2027-03-10", "2027-03-10", "Observance of Eid'l Fitr");
            AddClosure(context, "2027-03-24", "2027-03-24", "Holy Wednesday, Institutional Holiday");
            AddClosure(context, "2027-04-04", "2027-04-04", "Antipolo Cityhood Day");
            AddClosure(context, "2027-05-17", "2027-05-17", "Observance of Eid'l Adha");
            AddClosure(context, "2027-06-11", "2027-06-11", "Rizal Province Day");

            // First Trimester
            AddClosure(context, "2026-10-14", "2026-10-14", "Free Day and Associates' Mission Day");
            AddClosure(context, "2026-11-25", "2026-12-05", "Activity Ban");
            AddClosure(context, "2026-12-10", "2027-01-04", "Term Break");

            // Second Trimester
            AddClosure(context, "2027-02-23", "2027-02-23", "Free Day and Associates' Mission Day");
            AddClosure(context, "2027-04-07", "2027-04-07", "Free Day, Feast of Saint John Baptist De La Salle");
            AddClosure(context, "2027-04-12", "2027-04-17", "Activity Ban");
            AddClosure(context, "2027-04-25", "2027-05-02", "Term Break");

            // Third Trimester
            AddClosure(context, "2027-06-21", "2027-06-21", "Free Day and Associates' Mission Day");
            AddClosure(context, "2027-07-31", "2027-08-06", "Activity Ban");
            AddClosure(context, "2027-08-14", "2027-08-22", "Term Break");

            context.SaveChanges();
        }
    }

    // Finds the venue by name, then adds one approver row for it
    private static void AddApprover(ApplicationDbContext context, string venueName, string office, string email, int step)
    {
        var venue = context.Venue.FirstOrDefault(v => v.Name == venueName);

        if (venue == null)
        {
            return;
        }

        var approver = new VenueApprover();
        approver.VenueId = venue.Id;
        approver.Office = office;
        approver.Email = email;
        approver.StepOrder = step;

        context.VenueApprover.Add(approver);
    }

    // Adds one closure row, with the dates given as yyyy-MM-dd
    private static void AddClosure(ApplicationDbContext context, string start, string end, string reason)
    {
        var closure = new SchoolClosure();
        closure.StartDate = DateOnly.ParseExact(start, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        closure.EndDate = DateOnly.ParseExact(end, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        closure.Reason = reason;

        context.SchoolClosures.Add(closure);
    }
}