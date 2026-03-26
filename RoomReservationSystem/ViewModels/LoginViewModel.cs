
using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please Fill in your Username" )]
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please Fill in your Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
