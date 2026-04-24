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
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateReservationAsync(int id)
        {
            var reservation = await _reservationRepository.GetReservationByIdAsync(id);

            if (reservation == null)
                return NotFound("Reservation not found.");

            if (reservation.Status == ReservationStatus.Active)
                return BadRequest("Reservation is already active.");

            // collision check 
            bool hasCollision = await _reservationRepository.CheckCollisionAsync(
                reservation.RoomId,
                reservation.StartTime,
                reservation.EndTime,
                reservation.Id);

            if (hasCollision)
                return BadRequest("Room is already booked for this time slot.");

            reservation.Status = ReservationStatus.Active;

            await _reservationRepository.UpdateStatusAsync(id, ReservationStatus.Active);

            // 5. Zápis do historie (Repozitář udělá hloupý INSERT do tabulky historie) az nakonec projektu
            //await _reservationRepository.AddHistoryRecordAsync(id, ReservationStatus.Active, DateTime.Now);

            return Ok();
        }
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelReservationAsync(int id)
        {
            var reservation = await _reservationRepository.GetReservationByIdAsync(id);

            if (reservation == null)
                return NotFound("Reservation not found.");

            if (reservation.Status == ReservationStatus.Cancelled)
                return BadRequest("Reservation is already cancelled.");
            
            reservation.Status = ReservationStatus.Cancelled;   

            await _reservationRepository.UpdateStatusAsync(id, ReservationStatus.Cancelled);

            // 5. Zápis do historie (Repozitář udělá hloupý INSERT do tabulky historie) az nakonec projektu
            //await _reservationRepository.AddHistoryRecordAsync(id, ReservationStatus.Active, DateTime.Now);

            return Ok();
        }

    }
}
