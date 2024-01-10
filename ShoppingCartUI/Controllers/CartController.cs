using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShoppingCartUI.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartRepository _CartRepository;

        public CartController(ICartRepository cartRepository)
        {
            _CartRepository = cartRepository;
        }

        public async Task<IActionResult> AddItem(int laptopId, int quantity = 1, int redirect = 0)
        {
            var cartCount = await _CartRepository.AddItem(laptopId, quantity);
            if (redirect == 0)
            {
                return Ok(cartCount);
            }

            return RedirectToAction("GetUserCart");
        }

        public async Task<IActionResult> GetTotalItemInCart()
        {
            int cartItem = await _CartRepository.GetCartItemCount();
            return Ok(cartItem);
        }

        public async Task<IActionResult> GetUserCart()
        {
            var cart = await _CartRepository.GetUserCart();
            return View(cart);
        }

        public async Task<IActionResult> GoToCheckout()
        {
            var isCheckedOut = await _CartRepository.GoToCheckout();
            if (!isCheckedOut)
                return StatusCode(500);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> RemoveItem(int laptopId)
        {
            //var cartCount =
            await _CartRepository.RemoveItem(laptopId);
            return RedirectToAction("GetUserCart");
        }
    }
}