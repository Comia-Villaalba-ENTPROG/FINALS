using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FINALS.Models;

namespace FINALS.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{

public DbSet<FINALS.Models.Organization> Organization { get; set; } = default!;

public DbSet<FINALS.Models.Venue> Venue { get; set; } = default!;

public DbSet<FINALS.Models.Equipment> Equipment { get; set; } = default!;

public DbSet<FINALS.Models.VenueApprover> VenueApprover { get; set; } = default!;
}
