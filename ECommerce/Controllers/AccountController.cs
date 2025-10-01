using Microsoft.AspNetCore.Mvc;
using ECommerce.Models;
using ECommerce.Services;

namespace ECommerce.Controllers
{
    public class AccountController : Controller
    {
        private readonly JsonDataService _dataService;

        public AccountController(JsonDataService dataService)
        {
            _dataService = dataService;
        }

        
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _dataService.GetUserByUsername(username);

            if (user != null && user.Password == password)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role.ToString());

                if (user.Role == UserRole.Admin)
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }

        
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            var existingUser = _dataService.GetUserByUsername(user.Username);
            if (existingUser != null)
            {
                ViewBag.Error = "Bu kullanıcı adı zaten kullanılıyor!";
                return View();
            }

            user.Role = UserRole.User;
            _dataService.AddUser(user);

            return RedirectToAction("Login");
        }

      
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}