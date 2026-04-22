using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomReservatingSystem.Shared.DTOs
{
    public class ReservationDTO
    {

        public int Id { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }


        public int OrganizerId { get; set; }
        public string UserName { get; set; }


        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Purpose { get; set; }
        public int PersonCount { get; set; }
        public ReservationStatus Status { get; set; }
    }
}
