using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RoomReservationSystem.Controllers
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository _usersrRepository;
        private readonly IConfiguration _configuration;

        public AuthController(UserRepository usersrRepository, IConfiguration configuration)
        {
            _usersrRepository = usersrRepository;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _usersrRepository.GetUserByUsernameAsync(request.Username);
            //user not found or password does not match
            if (user == null || !BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.PasswordHash))
            {
                
                return Unauthorized(new { message = "Incorrect Username or Password" });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("UserId", user.Id.ToString()),
                new Claim("IsAdmin", user.IsAdmin.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? ""));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2), // token will be valid for 2 hours
                signingCredentials: creds
            );

            // Convert the token to the long encoded text
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Send it back to the desktop application
            return Ok(new { Token = tokenString });
        }
    }
}