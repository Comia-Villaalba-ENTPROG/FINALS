namespace FINALS.Models
{
    public class Organization
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactEmail { get; set; }
    }
}