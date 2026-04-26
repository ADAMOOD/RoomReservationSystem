using System;

namespace RoomReservatingSystem.Shared.DTOs
{
    public class ReservationHistoryDTO
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public ReservationStatus? OldStatus { get; set; }
        public ReservationStatus NewStatus { get; set; }
        public DateTime ChangedAt { get; set; }


        public string ChangedByUserName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
    }
}