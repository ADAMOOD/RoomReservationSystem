using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Repositories;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    public class ReservationsController : ControllerBase
    {
        private readonly ReservationRepository _reservationRepository;

        public ReservationsController(ReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReservationsAsync()
        {
            var reservations = await _reservationRepository.GetReservationsAsync();
            return Ok(reservations);
        }
    }
}
