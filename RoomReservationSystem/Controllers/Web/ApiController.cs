using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoomReservatingSystem.Shared;
using RoomReservationSystem.Repositories;
using System.Security.Claims;

namespace RoomReservationSystem.Controllers.Web
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
            int? currentUserId = null;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                currentUserId = int.Parse(User.FindFirstValue("UserId"));
            }

            int? filterId = null;
            if (onlyMine)
            {
                filterId = currentUserId;
            }
            var reservations = await _reservationRepository.GetFilteredReservationsAsync(roomId, purpose, hideCancelled, filterId);

            var calendarEvents = reservations.Select(r => new
            {
                id = r.Id,
                title = $"{r.RoomName} ({((currentUserId == null || r.OrganizerId != currentUserId) ? "Occupied" : r.Purpose)})",

                start = r.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = r.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                color = r.Status == ReservationStatus.Cancelled ? "#6c757d" : "#198754"
            });

            return Ok(calendarEvents);
        }


    }
}
