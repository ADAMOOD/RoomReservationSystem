using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Repositories;
using RoomReservationSystem.ViewModels;
using System.Diagnostics;
using System.Security.Claims;
using RoomReservatingSystem.Shared;

namespace RoomReservationSystem.Controllers.Web
{
    public class WebRoomsController:Controller
    {
        private readonly RoomRepository _roomRepository;
        private readonly ReservationRepository _reservationRepository;

        public WebRoomsController(RoomRepository roomRepository, ReservationRepository reservationRepository)
        {
            _roomRepository = roomRepository;
            _reservationRepository = reservationRepository;
        }
        public async Task<IActionResult> Index()
        {
            var rooms = await _roomRepository.GetAllRoomsAsync();
            return View(rooms);
        }

        [HttpGet]
        public async Task<IActionResult> GetRoomEvents(int roomId)
        {
            var reservations = await _reservationRepository.GetAllReservationsForRoomAsync(roomId);
            int? currentUserId = null;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                currentUserId = int.Parse(User.FindFirstValue("UserId"));
            }

            var calendarEvents = reservations.Select(r => new
            {
                title = (currentUserId==null||r.OrganizerId!=currentUserId)? "[Occupied]":r.Purpose,
                start = r.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"), // Important: ISO 8601 time format
                end = r.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                color = "#dc3545",
                allDay = false // times not all days
            });

            return Json(calendarEvents);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Book(CreateReservationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Get all errs and print them as long message
                var errorMessages = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["ErrorMessage"] = $"Reservation fault ({model.RoomName}): {errorMessages}";
                return RedirectToAction("Index"); 
            }
            var existingReservations = await _reservationRepository.GetAllReservationsForRoomAsync(model.RoomId);

            bool isOverlapping = existingReservations.Any(r =>
                r.StartTime < model.EndTime.Value &&
                r.EndTime > model.StartTime.Value);

            if (isOverlapping)
            {
                TempData["ErrorMessage"] = $"Sorry, but {model.RoomName} Is already booked at this time";
                return RedirectToAction("Index");
            }

            int loggedUserId = int.Parse(User.FindFirstValue("UserId"));//user is a shortcut for HttpContext.User which we have access due to the Controller inharitance
            Reservation newReservation = new Reservation()
            {
                RoomId = model.RoomId, 
                StartTime = model.StartTime.Value, // value because dates are nullable
                EndTime = model.EndTime.Value,
                OrganizerId = loggedUserId,
                PersonCount = model.PersonCount,
                Purpose = model.Purpose,
                Status = ReservationStatus.Active
            };
            await _reservationRepository.CreateReservationAsync(newReservation);

            //record history of reservation creation (if reservation creation was successful)
            int? newId = await _reservationRepository.CreateReservationAsync(newReservation);
            if (newId.HasValue)
            {
                await _reservationRepository.AddHistoryRecordAsync(newId.Value, null, ReservationStatus.Active, loggedUserId);
            }
            TempData["SuccessMessage"] = $"Reservation {model.RoomName} has been successfuly made!";
            return RedirectToAction("Index");
        }

    }
}
