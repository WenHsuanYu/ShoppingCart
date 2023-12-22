using Microsoft.AspNetCore.Mvc;
using ShoppingCartUI.Models.DTOs;
using System.Diagnostics;

namespace ShoppingCartUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeRepository _homeRepository;

        public HomeController(ILogger<HomeController> logger, IHomeRepository homeRepository)
        {
            _homeRepository = homeRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string searchText = "", int BrandId = 0)
        {
            var laptops = await _homeRepository.GetLaptopsAsync(searchText, BrandId);
            var brands = await _homeRepository.GetBrandsAsync();
            var laptopDTO = new LaptopDTO()
            {
                Laptops = laptops,
                Brands = brands,
                SearchText = searchText,
                BrandId = BrandId
            };
            return View(laptopDTO);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
