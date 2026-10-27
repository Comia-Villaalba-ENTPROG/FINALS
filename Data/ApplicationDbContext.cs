using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FINALS.Models;

namespace FINALS.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<Equipment> Equipments { get; set; }
    public DbSet<VenueApprover> VenueApprovers { get; set; }
}
