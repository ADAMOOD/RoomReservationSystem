using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservatingSystem.Shared;
using RoomReservationSystem.Repositories;

namespace RoomReservationSystem.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
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
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateRoomAsync([FromBody] Room newRoom)
        {
            await _repository.AddRoomAsync(newRoom);
            return Ok();
        }
    }
}