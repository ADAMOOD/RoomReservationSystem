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
                RoomName = room.Name
            };
            return View(newReservation);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Book(CreateReservationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            int loggedUserId = int.Parse(User.FindFirstValue("UserId"));//user is a shortcut for HttpContext.User which we have access due to the Controller inharitance
            Reservation newReservation = new Reservation()
            {
                RoomId = model.RoomId, 
                StartTime = model.StartTime.Value, // value becose dates are nullable
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
