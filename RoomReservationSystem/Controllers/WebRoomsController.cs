using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Repositories;
using System.Diagnostics;

namespace RoomReservationSystem.Controllers
{
    public class WebRoomsController:Controller
    {
        private readonly RoomRepository _repository;
        public WebRoomsController(RoomRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Získáme data z databáze
            var rooms = await _repository.GetAllRoomsAsync();
            return View(rooms);
        }
    }
}
