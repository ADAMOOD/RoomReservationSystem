using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservatingSystem.Shared;
using RoomReservationSystem.Repositories;
using RoomReservationSystem.ViewModels;
using System.Reflection;
using System.Security.Claims;

namespace RoomReservationSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly ReservationRepository _reservationRepository;

        public AccountController(UserRepository userRepository, ReservationRepository reservationRepository)
        {
            _userRepository = userRepository;
            _reservationRepository = reservationRepository;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Unsign Cookie from the browser
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            int loggedUserId = int.Parse(User.FindFirstValue("UserId"));
            var reservations = await _reservationRepository.GetReservationsWithRoomNameByUserAsync(loggedUserId);
            return View(reservations);
        }

        [Authorize]
        [HttpGet] 
        public async Task<IActionResult> CancelReservation(int id)
        {
            int loggedUserId = int.Parse(User.FindFirstValue("UserId"));

            var reservation = await _reservationRepository.GetReservationByIdAsync(id);

            if (reservation == null)
            {
                return NotFound();
            }

            if (reservation.OrganizerId != loggedUserId)
            {
                return Forbid(); // Return error 403
            }

            // already cancelled, no need to do it again, just redirectback to profile
            if (reservation.Status == ReservationStatus.Cancelled)
            {
                return RedirectToAction("Profile");
            }

            reservation.Status = ReservationStatus.Cancelled;
            await _reservationRepository.UpdateReservationAsync(reservation);

            TempData["SuccessMessage"] = "Reservation has been successfully canceled";
            return RedirectToAction("Profile");
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _userRepository.GetUserByUsernameAsync(model.Username) != null)
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

            await _userRepository.CreateUserAsync(newUser);
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User dbUser = await _userRepository.GetUserByUsernameAsync(model.Username);
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

    }
}