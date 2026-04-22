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

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoomAsync(int id)
        {
            bool isDeleted = await _repository.DeleteAsync(id);

            if (isDeleted)
            {
                return NoContent(); // code 204, indicating successful deletion with no content to return
            }
            else
            {
                return BadRequest("This room cannot be Deleted.");
            }
        }
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoomAsync(int id, [FromBody] Room updatedRoom)
        {
            //ensure that the Id in the URL matches the Id in the body (if body contains Id)
            updatedRoom.Id = id;
            bool success = await _repository.UpdateRoomAsync(updatedRoom);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}