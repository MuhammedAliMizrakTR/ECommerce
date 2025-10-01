using Microsoft.AspNetCore.Mvc;
using ECommerce.Models;
using ECommerce.Services;

namespace ECommerce.Controllers
{
    public class AdminController : Controller
    {
        private readonly JsonDataService _dataService;
        private readonly IWebHostEnvironment _environment;

        public AdminController(JsonDataService dataService, IWebHostEnvironment environment)
        {
            _dataService = dataService;
            _environment = environment;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // Dashboard
        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.ProductCount = _dataService.GetProducts().Count;
            ViewBag.UserCount = _dataService.GetUsers().Count;
            ViewBag.OrderCount = _dataService.GetOrders().Count;

            return View();
        }

        // Product Management
        public IActionResult Products()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var products = _dataService.GetProducts();
            return View(products);
        }

        [HttpGet]
        public IActionResult CreateProduct()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product, IFormFile imageFile)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            // Resim yükleme
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                product.ImageUrl = "/uploads/" + uniqueFileName;
            }
            else
            {
                product.ImageUrl = "/uploads/no-image.jpg";
            }

            _dataService.AddProduct(product);
            TempData["Success"] = "Ürün başarıyla eklendi!";
            return RedirectToAction("Products");
        }

        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var product = _dataService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Product product, IFormFile imageFile)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var existingProduct = _dataService.GetProductById(product.Id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            // Yeni resim yüklendiyse
            if (imageFile != null && imageFile.Length > 0)
            {
                // Eski resmi sil (eğer default değilse)
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl) &&
                    existingProduct.ImageUrl != "/uploads/no-image.jpg" &&
                    existingProduct.ImageUrl.StartsWith("/uploads/"))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, existingProduct.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                product.ImageUrl = "/uploads/" + uniqueFileName;
            }
            else
            {
                // Resim değişmedi, eskisini koru
                product.ImageUrl = existingProduct.ImageUrl;
            }

            _dataService.UpdateProduct(product);
            TempData["Success"] = "Ürün başarıyla güncellendi!";
            return RedirectToAction("Products");
        }

        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var product = _dataService.GetProductById(id);
            if (product != null)
            {
                // Resmi sil (eğer default değilse)
                if (!string.IsNullOrEmpty(product.ImageUrl) &&
                    product.ImageUrl != "/uploads/no-image.jpg" &&
                    product.ImageUrl.StartsWith("/uploads/"))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }

            _dataService.DeleteProduct(id);
            TempData["Success"] = "Ürün başarıyla silindi!";
            return RedirectToAction("Products");
        }

        // User Management
        public IActionResult Users()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var users = _dataService.GetUsers();
            return View(users);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        public IActionResult CreateUser(User user, string role)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var existingUser = _dataService.GetUserByUsername(user.Username);
            if (existingUser != null)
            {
                ViewBag.Error = "Bu kullanıcı adı zaten kullanılıyor!";
                return View();
            }

            user.Role = role == "Admin" ? UserRole.Admin : UserRole.User;
            _dataService.AddUser(user);

            TempData["Success"] = "Kullanıcı başarıyla eklendi!";
            return RedirectToAction("Users");
        }
    }
}