using Microsoft.AspNetCore.Mvc;
using RoomReservatingSystem.Shared;
using RoomReservationSystem.Repositories;
using RoomReservationSystem.ViewModels;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace RoomReservationSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _repository;

        public AccountController(UserRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _repository.GetUserByUsernameAsync(model.Username) != null)
            {
                ModelState.AddModelError("", "User with this name already exists");
                return View(model);
            }

            User newUser = new User
            {
                Username = model.Username,
                PasswordHash = model.Password,
                IsAdmin = false
            };

            await _repository.CreateUserAsync(newUser);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User dbUser = await _repository.GetUserByUsernameAsync(model.Username);
            if (dbUser == null)
            {
                ModelState.AddModelError("", "User with this username does not exist");
                return View(model);
            }

            if (!BCrypt.Net.BCrypt.EnhancedVerify(model.Password, dbUser.PasswordHash))
            {
                ModelState.AddModelError("", "Wrong password");
                return View(model);
            }
            // Create Claims for the User
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, dbUser.Username),
                new Claim("UserId", dbUser.Id.ToString()),
                new Claim(ClaimTypes.Role, dbUser.IsAdmin ? "Admin" : "User")
            };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            // Assign the cookie to the browser
            await HttpContext.SignInAsync("Cookies", principal);

            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Unsign Cookie from the browser
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }
    }
}