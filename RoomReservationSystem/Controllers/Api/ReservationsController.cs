using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservatingSystem.Shared;
using RoomReservationSystem.Repositories;
using System.Security.Claims;

namespace RoomReservationSystem.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    public class ReservationsController : ControllerBase
    {
        private readonly ReservationRepository _reservationRepository;

        private readonly RoomRepository _roomRepository; 

        public ReservationsController(ReservationRepository reservationRepository, RoomRepository roomRepository)
        {
            _reservationRepository = reservationRepository;
            _roomRepository = roomRepository;
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
            //first we take the user from the token, then we create the reservation and at the end we add the history record (if reservation creation was successful)
            int loggedUserId = int.Parse(User.FindFirstValue("UserId"));
            // collision check
            int? newId = await _reservationRepository.CreateReservationAsync(newReservation);

            // if newId is null, it means that there was a collision and the reservation was not created, so we return a bad request with a message
            if (newId.HasValue)
            {
                await _reservationRepository.AddHistoryRecordAsync(newId.Value, null, ReservationStatus.Active, loggedUserId);
            }

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
            if (reservation == null) return NotFound("Reservation not found.");
            if (reservation.Status == ReservationStatus.Active) return BadRequest("Reservation is already active.");

            // again we need to get the room to check the capacity and max reservation time,
            // because these rules also apply when activating a reservation (not only when creating or updating it)
            var room = await _roomRepository.GetRoomByIdAsync(reservation.RoomId);
            if (room != null)
            {
                if (reservation.PersonCount > room.Capacity)
                    return BadRequest($"Cannot activate: Room capacity is only {room.Capacity} people.");

                var duration = (reservation.EndTime - reservation.StartTime).TotalMinutes;
                if (duration > room.MaxReservationMinutes)
                    return BadRequest($"Cannot activate: Max reservation time for this room is {room.MaxReservationMinutes} minutes.");
            }
            // collision check
            bool hasCollision = await _reservationRepository.CheckCollisionAsync(
                reservation.RoomId,
                reservation.StartTime,
                reservation.EndTime,
                reservation.Id);

            if (hasCollision) return BadRequest("Room is already booked for this time slot.");

            // change status in DB
            await _reservationRepository.UpdateStatusAsync(id, ReservationStatus.Active);

            // Zápis do historie
            int loggedUserId = int.Parse(User.FindFirstValue("UserId"));
            await _reservationRepository.AddHistoryRecordAsync(id, ReservationStatus.Cancelled, ReservationStatus.Active, loggedUserId);

            return Ok();
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelReservationAsync(int id)
        {
            var reservation = await _reservationRepository.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound("Reservation not found.");
            if (reservation.Status == ReservationStatus.Cancelled) return BadRequest("Reservation is already cancelled.");

            // change status in DB
            await _reservationRepository.UpdateStatusAsync(id, ReservationStatus.Cancelled);

            // record history
            int loggedUserId = int.Parse(User.FindFirstValue("UserId"));
            await _reservationRepository.AddHistoryRecordAsync(id, ReservationStatus.Active, ReservationStatus.Cancelled, loggedUserId);

            return Ok();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("history")]
        public async Task<IActionResult> GetReservationHistoryAsync()
        {
            var history = await _reservationRepository.GetReservationHistoryAsync();
            return Ok(history);
        }

    }
}
