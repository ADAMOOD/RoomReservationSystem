using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomReservatingSystem.Shared
{
    public class Room
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity {  get; set; }
        public string Equipment {  get; set; } = string.Empty;
        public int MaxReservationMinutes { get; set; }
    }
}
