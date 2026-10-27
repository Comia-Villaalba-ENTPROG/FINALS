using System.ComponentModel.DataAnnotations;

namespace FINALS.Models;

public class Organization
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Type { get; set; } = "";

    public bool IsActive { get; set; } = true; 

}