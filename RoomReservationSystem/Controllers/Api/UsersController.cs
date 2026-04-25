using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Repositories;
using System.Threading.Tasks;

namespace RoomReservationSystem.Controllers.Api
{
    [Authorize(Policy = "AdminOnly")]
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        public UsersController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAsync(int id)
        {
            // First, fetch the user to ensure they exist and aren't an admin
            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (user.IsAdmin)
            {
                return BadRequest("Administrator accounts cannot be deleted.");
            }

            // Perform the soft delete operation
            bool success = await _userRepository.SoftDeleteUserAsync(id);

            if (!success)
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }

            return NoContent(); 
        }
    }
}