using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FINALS.Models;

public class ReservationEquipment
{
    [Key]
    public int Id { get; set; }

    public int ReservationId { get; set; }

    [ForeignKey("ReservationId")]
    public Reservation? Reservation { get; set; }

    public int EquipmentId { get; set; }

    [ForeignKey("EquipmentId")]
    public Equipment? Equipment { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; }
}