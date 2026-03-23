namespace RoomReservatingSystem.Shared;
using System.ComponentModel.DataAnnotations;
public class ReservationHistory
{
    [Key]
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public ReservationStatus OldStatus { get; set; }
    public ReservationStatus NewStatus { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.Now;
    public int ChangedByUserId { get; set; }
}