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
            AddApprover(context, "Benilde Hall", "GASO", "gaso@benilde.edu.ph", 1);

            AddApprover(context, "Chapel", "OLM", "olm@benilde.edu.ph", 1);
            AddApprover(context, "Chapel", "LMSL", "lmsl@benilde.edu.ph", 2);
            AddApprover(context, "Chapel", "GASO", "gaso@benilde.edu.ph", 3);

            AddApprover(context, "Lobby", "GASO", "gaso@benilde.edu.ph", 1);

            AddApprover(context, "Function Hall", "HM", "hm@benilde.edu.ph", 1);
            AddApprover(context, "Function Hall", "GASO", "gaso@benilde.edu.ph", 2);

            AddApprover(context, "GYM A", "GASO", "gaso@benilde.edu.ph", 1);
            AddApprover(context, "GYM B", "GASO", "gaso@benilde.edu.ph", 1);
            AddApprover(context, "GYM C", "GASO", "gaso@benilde.edu.ph", 1);

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
}