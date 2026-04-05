using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Repositories;
using RoomReservationSystem.ViewModels;
using System.Diagnostics;
using System.Security.Claims;
using RoomReservatingSystem.Shared;

namespace RoomReservationSystem.Controllers
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
        [HttpGet]
        public async Task<IActionResult> Book(int id)
        {
            var room = await _roomRepository.GetRoomByIdAsync(id);

            if (room == null)
            {
                return NotFound();
            }
            CreateReservationViewModel newReservation = new CreateReservationViewModel()
            {
                RoomId = id,
                RoomName = room.Name,
                RoomCapacity = room.Capacity,
                MaxReservationMinutes = room.MaxReservationMinutes
            };
            return View(newReservation);
        }
        [HttpGet]
        public async Task<IActionResult> GetRoomEvents(int roomId)
        {
            var reservations = await _reservationRepository.GetAllReservationsForRoomAsync(roomId);

            var calendarEvents = reservations.Select(r => new
            {
                title = r.Purpose,
                start = r.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"), // Important: ISO 8601 formát času
                end = r.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                color = "#dc3545",
                allDay = false // times not all days
            });

            return Json(calendarEvents);
        }
        //TEST
        [HttpGet]
        public IActionResult CalendarTest(int roomId)
        {
            ViewBag.RoomId = roomId; // Tímto jednoduše pošleme ID do HTML
            return View();
        }
        //TEST

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Book(CreateReservationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var existingReservations = await _reservationRepository.GetAllReservationsForRoomAsync(model.RoomId);

            bool isOverlapping = existingReservations.Any(r =>
                r.StartTime < model.EndTime.Value &&
                r.EndTime > model.StartTime.Value);

            if (isOverlapping)
            {
                ModelState.AddModelError("", "Sorry, but there is already a reservation at this time");
                return View(model);
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
            return RedirectToAction("Index");
        }

    }
}
