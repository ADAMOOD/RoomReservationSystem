using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservatingSystem.Shared;
using RoomReservationSystem.Repositories;

namespace RoomReservationSystem.Controllers.Api
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
        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> GetAllReservationsAsync()
        {
            var reservations = await _reservationRepository.GetReservationDTOsAsync();
            return Ok(reservations);
        }
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservationAsync(int id)
        {
            var deleted = await _reservationRepository.DeleteReservationAsync(id);
            if (deleted)
            {
                return NoContent(); // code 204, indicating successful deletion with no content to return
            }
            else
            {
                return BadRequest("This reservation cannot be Deleted.");
            }
        }
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateReservationsAsync([FromBody] Reservation newReservation)
        {
            await _reservationRepository.CreateReservationAsync(newReservation);
            return Ok();
        }
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservationAsync(int id, [FromBody] Reservation updatedReservation)
        {
            //ensure that the Id in the URL matches the Id in the body (if body contains Id)
            updatedReservation.Id = id;
            bool success = await _reservationRepository.UpdateRoomAsync(updatedReservation);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

    }
}
