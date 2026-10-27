using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FINALS.Models;

public class VenueApprover
{
    [Key]
    public int Id { get; set; }

    public int VenueId { get; set; }

    [ForeignKey("VenueId")]
    public Venue? Venue { get; set; }

    public string Office { get; set; } = "";

    public string Email { get; set; } = "";

    public int StepOrder { get; set; }
}