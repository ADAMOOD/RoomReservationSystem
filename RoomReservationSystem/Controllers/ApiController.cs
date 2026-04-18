using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoomReservatingSystem.Shared;
using RoomReservationSystem.Repositories;
using System.Security.Claims;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ReservationRepository _reservationRepository;

        public ApiController(ReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;

        }
        [HttpGet("Api/GetGlobalEvents")]
        public async Task<IActionResult> GetGlobalEvents(int roomId, string? purpose, bool hideCancelled, bool onlyMine)
        {
            int? loggedUserId = null;
            if (onlyMine && User.Identity != null && User.Identity.IsAuthenticated)
            {
                loggedUserId = int.Parse(User.FindFirstValue("UserId"));
            }
            var reservations = await _reservationRepository.GetFilteredReservationsAsync(roomId, purpose, hideCancelled, loggedUserId);

            var calendarEvents = reservations.Select(r => new
            {
                id = r.Id,
                title = $"{r.RoomName} ({r.Purpose})",

                start = r.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = r.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                color = r.Status == ReservationStatus.Cancelled ? "#6c757d" : "#198754"
            });

            return Ok(calendarEvents);
        }


    }
}
