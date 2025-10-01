using Microsoft.AspNetCore.Mvc;
using ECommerce.Models;
using ECommerce.Services;
using System.Text.Json;

namespace ECommerce.Controllers
{
    public class CartController : Controller
    {
        private readonly JsonDataService _dataService;

        public CartController(JsonDataService dataService)
        {
            _dataService = dataService;
        }

        private Cart GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
            {
                return new Cart();
            }
            return JsonSerializer.Deserialize<Cart>(cartJson);
        }

        private void SaveCart(Cart cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("Cart", cartJson);
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = _dataService.GetProductById(productId);
            if (product == null || product.Stock < quantity)
            {
                TempData["Error"] = "Ürün stokta yok veya yeterli stok yok!";
                return RedirectToAction("Index", "Home");
            }

            var cart = GetCart();
            cart.UserId = userId.Value;

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }

            SaveCart(cart);
            TempData["Success"] = "Ürün sepete eklendi!";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    cart.Items.Remove(item);
                }
                SaveCart(cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.Items.RemoveAll(i => i.ProductId == productId);
            SaveCart(cart);

            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCart();
            if (!cart.Items.Any())
            {
                return RedirectToAction("Index");
            }

            return View(cart);
        }

        [HttpPost]
        public IActionResult ProcessPayment(string cardNumber, string cardName, string expiryDate, string cvv)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCart();
            if (!cart.Items.Any())
            {
                return RedirectToAction("Index");
            }

            
            var order = new Order
            {
                UserId = userId.Value,
                TotalAmount = cart.TotalPrice,
                Status = "Completed"
            };

            foreach (var item in cart.Items)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    Quantity = item.Quantity
                });

                
                var product = _dataService.GetProductById(item.ProductId);
                if (product != null)
                {
                    product.Stock -= item.Quantity;
                    _dataService.UpdateProduct(product);
                }
            }

            _dataService.AddOrder(order);

            
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("OrderSuccess", new { orderId = order.Id });
        }

        public IActionResult OrderSuccess(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.OrderId = orderId;
            return View();
        }
    }
}
