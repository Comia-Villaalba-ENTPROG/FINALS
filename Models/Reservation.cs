using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FINALS.Models;

public class Reservation
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Tracking Number")]
    public string TrackingNumber { get; set; } = "";

    public string ApprovalToken { get; set; } = "";

    [Required]
    [Display(Name = "Organization")]
    public int OrganizationId { get; set; }

    [ForeignKey("OrganizationId")]
    public Organization? Organization { get; set; }

    [Required]
    [Display(Name = "Venue")]
    public int VenueId { get; set; }

    [ForeignKey("VenueId")]
    public Venue? Venue { get; set; }

    [Required]
    [StringLength(150)]
    [Display(Name = "Activity Name")]
    public string ActivityName { get; set; } = "";

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Use")]
    public DateOnly ReservationDate { get; set; }

    [Required]
    [DataType(DataType.Time)]
    [Display(Name = "Start Time")]
    public TimeOnly StartTime { get; set; }

    [Required]
    [DataType(DataType.Time)]
    [Display(Name = "End Time")]
    public TimeOnly EndTime { get; set; }

    [Required]
    [Range(1, 1000)]
    [Display(Name = "Expected Attendees")]
    public int ExpectedAttendees { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Requested By")]
    public string RequesterName { get; set; } = "";

    [Required]
    [EmailAddress]
    [RegularExpression(@"^[a-z0-9._%+-]+@antipolo\.benilde\.edu\.ph$",
    ErrorMessage = "Email must be in lowercase and use the format name@antipolo.benilde.edu.ph.")]
    [Display(Name = "Email Address")]
    public string RequesterEmail { get; set; } = "";

        [Required]
    [RegularExpression(@"^09\d{9}$",
        ErrorMessage = "Contact number must be 11 digits starting with 09.")]
    [Display(Name = "Contact Number")]
    public string RequesterContact { get; set; } = "";

    [Display(Name = "Status")]
    public string Status { get; set; } = "Pending";

    [Display(Name = "Current Approval Step")]
    public int CurrentStep { get; set; } = 1;

    [Display(Name = "Date Submitted")]
    public DateTime DateSubmitted { get; set; } = DateTime.Now;

}