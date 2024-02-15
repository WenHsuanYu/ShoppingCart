using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

        [HttpPost]
        public async Task<IActionResult> AddItem([Bind("LaptopId, Redirect"), FromBody] ItemRequest request)
        {
            var cartCount = await _CartRepository.AddItem(request.LaptopId, request.Quantity);
            if (request.Redirect == 0)
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

        [HttpPost]
        public async Task<IActionResult> GoToCheckout()
        {
            var isCheckedOut = await _CartRepository.GoToCheckout();
            if (!isCheckedOut)
                return StatusCode(500);
            //return RedirectToAction("Index", "Home");
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveItem([Bind("LaptopId"), FromBody] ItemRequest request)
        {
            //var cartCount =
            await _CartRepository.RemoveItem(request.LaptopId);
            return RedirectToAction("GetUserCart");
        }
    }

    public class ItemRequest
    {
        public int LaptopId { get; set; }
        public int Quantity { get; set; } = 1;
        public int Redirect { get; set; } = 0;
    }
}