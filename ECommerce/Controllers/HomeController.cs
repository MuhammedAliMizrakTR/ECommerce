using Microsoft.AspNetCore.Mvc;
using ECommerce.Services;

namespace ECommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly JsonDataService _dataService;

        public HomeController(JsonDataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult Index()
        {
            var products = _dataService.GetProducts();
            return View(products);
        }

        public IActionResult ProductDetail(int id)
        {
            var product = _dataService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
    }
}
