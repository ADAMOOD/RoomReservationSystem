using Microsoft.AspNetCore.Mvc;
using RoomReservatingSystem.Shared;
using RoomReservationSystem.Repositories;
using RoomReservationSystem.ViewModels;

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
    }
}