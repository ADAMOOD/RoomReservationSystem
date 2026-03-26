using System.ComponentModel.DataAnnotations;

namespace RoomReservatingSystem.Shared;
public enum ReservationStatus
{
    Active,
    Cancelled
}
public class Reservation
{
    [Key]
    public int Id { get; set; }
    public int RoomId { get; set; }
    public int OrganizerId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Purpose { get; set; }
    public int PersonCount { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;
}