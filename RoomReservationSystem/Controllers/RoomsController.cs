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
        public async Task<IActionResult> GetAllRoomsAsync()
        {
            var rooms = await _repository.GetAllRoomsAsync();
            return Ok(rooms);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoomAsync([FromBody] Room newRoom)
        {
            await _repository.AddRoomAsync(newRoom);
            return Ok();
        }
    }
}