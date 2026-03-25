using System.ComponentModel.DataAnnotations;

namespace RoomReservatingSystem.Shared
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public bool AsAdmin { get; set; } = false;
    }
}