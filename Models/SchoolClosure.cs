using System.ComponentModel.DataAnnotations;

namespace FINALS.Models;

public class SchoolClosure
{
    [Key]
    public int Id { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateOnly StartDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateOnly EndDate { get; set; }

    [Required]
    [StringLength(150)]
    [Display(Name = "Reason")]
    public string Reason { get; set; } = "";
}