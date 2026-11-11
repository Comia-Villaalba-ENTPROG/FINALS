namespace FINALS.Models
{
    // The shape of one holiday from the outside website (date.nager.at).
    // The names match the JSON it sends back, for example:
    // { "date": "2026-01-01", "localName": "Bagong Taon", "name": "New Year's Day" }
    public class Holiday
    {
        public DateOnly Date { get; set; }
        public string LocalName { get; set; } = "";   // name in Filipino
        public string Name { get; set; } = "";        // name in English
    }
}