using System.ComponentModel.DataAnnotations;

namespace FINALS.Models;

public class Equipment
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = ""; 

    public int TotalQuantity { get; set; } 

    public int MaxPerRequest { get; set;}
}