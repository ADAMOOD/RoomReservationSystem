using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomReservatingSystem.Shared.DTOs
{
    public class RoomStatisticsDTO
    {
        public string RoomName { get; set; } = string.Empty;

        public int? TotalReservations { get; set; }
        public double? TotalMinutesReserved { get; set; }
    }
}
