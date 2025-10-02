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

        
        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.ProductCount = _dataService.GetProducts().Count;
            ViewBag.UserCount = _dataService.GetUsers().Count;
            ViewBag.OrderCount = _dataService.GetOrders().Count;

            var orders = _dataService.GetOrders();
            ViewBag.TotalRevenue = orders.Sum(o => o.TotalAmount);

            return View();
        }

        

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

            
            if (imageFile != null && imageFile.Length > 0)
            {
                
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

        

        public IActionResult Orders()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = _dataService.GetOrders().OrderByDescending(o => o.OrderDate).ToList();

            
            foreach (var order in orders)
            {
                var user = _dataService.GetUserById(order.UserId);
                ViewData[$"User_{order.UserId}"] = user?.FullName ?? "Bilinmeyen Kullanıcı";
            }

            return View(orders);
        }

        [HttpGet]
        public IActionResult OrderDetail(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var order = _dataService.GetOrderById(id);
            if (order == null)
            {
                return NotFound();
            }

            var user = _dataService.GetUserById(order.UserId);
            ViewBag.UserName = user?.FullName ?? "Bilinmeyen Kullanıcı";
            ViewBag.UserEmail = user?.Email ?? "";

            return View(order);
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int id, string status)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var order = _dataService.GetOrderById(id);
            if (order != null)
            {
                order.Status = status;
                _dataService.UpdateOrder(order);
                TempData["Success"] = "Sipariş durumu güncellendi!";
            }

            return RedirectToAction("OrderDetail", new { id });
        }

        [HttpPost]
        public IActionResult DeleteOrder(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            _dataService.DeleteOrder(id);
            TempData["Success"] = "Sipariş başarıyla silindi!";
            return RedirectToAction("Orders");
        }

        

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
                return View(user);
            }

            user.Role = role == "Admin" ? UserRole.Admin : UserRole.User;
            _dataService.AddUser(user);

            TempData["Success"] = "Kullanıcı başarıyla eklendi!";
            return RedirectToAction("Users");
        }

        [HttpGet]
        public IActionResult EditUser(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _dataService.GetUserById(id);
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı!";
                return RedirectToAction("Users");
            }

            
            user.Password = string.Empty;

            return View(user);
        }

       
        [HttpPost]
        public IActionResult EditUser(User user, string role)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var existingUser = _dataService.GetUserById(user.Id);
            if (existingUser == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı!";
                return RedirectToAction("Users");
            }

            
            if (!ModelState.IsValid)
            {
                
                user.Password = string.Empty;
                return View(user);
            }

            
            if (existingUser.Username != user.Username)
            {
                var duplicateUser = _dataService.GetUserByUsername(user.Username);
                if (duplicateUser != null)
                {
                    ViewBag.Error = "Bu kullanıcı adı zaten kullanılıyor!";
                    user.Password = string.Empty; 
                    user.Role = existingUser.Role; 
                    return View(user);
                }
            }

            
            UserRole newRole = role == "Admin" ? UserRole.Admin : UserRole.User;
            user.Role = newRole;

            
            var allUsers = _dataService.GetUsers();
            var adminCount = allUsers.Count(u => u.Role == UserRole.Admin);

            
            if (existingUser.Role == UserRole.Admin && user.Role == UserRole.User && adminCount <= 1)
            {
                ViewBag.Error = "Sistemdeki son admin kullanıcının rolünü 'Kullanıcı' yapamazsınız! Sistemde en az bir admin olmalıdır.";

                
                user.Password = string.Empty;
                user.Role = existingUser.Role;
                return View(user);
            }

            
            if (string.IsNullOrEmpty(user.Password))
            {
                user.Password = existingUser.Password;
            }

            
            user.CreatedAt = existingUser.CreatedAt;

            
            _dataService.UpdateUser(user);

            TempData["Success"] = "Kullanıcı başarıyla güncellendi!";
            return RedirectToAction("Users");
        }


        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _dataService.GetUserById(id);
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı!";
                return RedirectToAction("Users");
            }

            
            var allUsers = _dataService.GetUsers();
            var adminCount = allUsers.Count(u => u.Role == UserRole.Admin);

            
            if (user.Role == UserRole.Admin && adminCount <= 1)
            {
                TempData["Error"] = "Son admin kullanıcıyı silemezsiniz! Sistemde en az bir admin olmalıdır.";
                return RedirectToAction("Users");
            }

            
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == id)
            {
                TempData["Error"] = "Kendi hesabınızı silemezsiniz!";
                return RedirectToAction("Users");
            }

            _dataService.DeleteUser(id);
            TempData["Success"] = "Kullanıcı başarıyla silindi!";
            return RedirectToAction("Users");
        }
    }
}