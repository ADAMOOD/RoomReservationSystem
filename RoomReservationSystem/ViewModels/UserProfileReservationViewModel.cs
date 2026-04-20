using RoomReservatingSystem.Shared;

namespace RoomReservationSystem.ViewModels
{
    public class UserProfileReservationViewModel
    {

        public int Id { get; set; }
        public int OrganizerId { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public int PersonCount { get; set; }
        public ReservationStatus Status { get; set; }
    }
}