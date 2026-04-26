using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservatingSystem.Shared;
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

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateUserAsync([FromBody] User newUser)
        {
            var resultId = await _userRepository.CreateUserAsync(newUser);

            if (resultId == null)
            {
                return Conflict("Username is already taken.");
            }

            return Ok();
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
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserAsync(int id, [FromBody] User updatedUser)
        {
            if (id != updatedUser.Id)
            {
                return BadRequest("User ID mismatch.");
            }

            var existingUser = await _userRepository.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound("User not found.");
            }
            bool success = await _userRepository.UpdateUserAsync(updatedUser);

            if (success)
            {
                return Ok();
            }

            return StatusCode(500, "An error occurred while updating the user.");
        }
    }
}