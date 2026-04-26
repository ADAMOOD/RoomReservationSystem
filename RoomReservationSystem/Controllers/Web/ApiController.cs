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

            // PŘEDÁVÁME currentUserId a onlyMine JAKO SAMOSTATNÉ PARAMETRY
            var reservations = await _reservationRepository.GetFilteredReservationsAsync(roomId, purpose, hideCancelled, onlyMine, currentUserId);

            var calendarEvents = reservations.Select(r =>
            {
                string eventColor;
                if (r.Status == ReservationStatus.Cancelled)
                {
                    eventColor = "#6c757d";
                }
                else if (currentUserId.HasValue && r.OrganizerId == currentUserId.Value)
                {
                    eventColor = "#0d6efd"; // Modrá pro moje rezervace
                }
                else
                {
                    eventColor = "#dc3545"; // Červená pro cizí
                }

                return new
                {
                    id = r.Id,
                    title = $"{r.RoomName} ({((currentUserId == null || r.OrganizerId != currentUserId) ? "Occupied" : r.Purpose)})",
                    start = r.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = r.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    color = eventColor
                };
            });

            return Ok(calendarEvents);
        }


    }
}
