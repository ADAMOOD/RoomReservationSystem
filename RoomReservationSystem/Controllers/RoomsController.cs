using Microsoft.AspNetCore.Mvc;
using RoomReservatingSystem.Shared;
using RoomReservationSystem.Repositories;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly RoomRepository _repository;
        public RoomsController(RoomRepository repository)
        {
            _repository = repository;
        }
        [HttpGet]
        public IActionResult GetAllRooms()
        {
            var rooms = _repository.GetAllRooms();
            return Ok(rooms);
        }

        [HttpPost]
        public IActionResult CreateRoom([FromBody] Room newRoom)
        {
            _repository.AddRoom(newRoom);
            return Ok();
        }
    }
}