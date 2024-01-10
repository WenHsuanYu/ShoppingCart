using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShoppingCartUI.Controllers
{
    [Authorize]
    public class UserOrderController : Controller
    {
        private readonly IUserOrderRepository _userOrderRepository;

        public UserOrderController(IUserOrderRepository userOrderRepository)
        {
            _userOrderRepository = userOrderRepository;
        }

        public async Task<IActionResult> UserOrders()
        {
            var orders = await _userOrderRepository.GetUserOrders();
            return View(orders);
        }
    }
}