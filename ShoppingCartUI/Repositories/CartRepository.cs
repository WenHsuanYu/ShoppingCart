using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace ShoppingCartUI.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        public CartRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<bool> AddItem(int LaptopId, int quantity)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                string? userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("The User is not logged in");

                var shoppingCart = await GetShoppingCart(userId);
                if (shoppingCart == null)
                {
                    shoppingCart = new ShoppingCart
                    {
                        UserId = userId
                    };
                    _context.ShoppingCarts.Add(shoppingCart);
                }
                _context.SaveChanges();
                // cart details
                var cartDetail = _context.CartDetails.FirstOrDefault(x => x.LaptopId == LaptopId && 
                    x.ShoppingCartId == shoppingCart.Id);
                if (cartDetail is not null)
                {
                    cartDetail.Quantity += quantity;
                    _context.CartDetails.Update(cartDetail);
                }
                else
                {
                    cartDetail = new CartDetail
                    {
                        ShoppingCartId = shoppingCart.Id,
                        LaptopId = LaptopId,
                        Quantity = quantity,
                    };
                    _context.CartDetails.Add(cartDetail);
                }
                _context.SaveChanges();
                transaction.Commit();
                return true;

            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }


        //public async Task<int> RemoveItem(int LaptopId)
        public async Task<bool> RemoveItem(int LaptopId)
        {
            try
            {
                string? userId = GetUserId();
                if (string.IsNullOrEmpty(userId)) {
                    throw new Exception("The User is not logged in");
                 
                }   
                var shoppingCart = await GetShoppingCart(userId) ?? throw new Exception("Invalid cart");
                // cart items detail
                var items = _context.CartDetails.FirstOrDefault(x => x.LaptopId == LaptopId &&
                    x.ShoppingCartId == shoppingCart.Id) ?? throw new Exception("No items in the cart");
                if (items.Quantity == 1)
                {
                    _context.CartDetails.Remove(items);
                }
                else
                {
                    items.Quantity--;
                }
                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            return true;

        }
        public async Task<ShoppingCart?> GetUserCart()
        {
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return null;
            var shoppingCart = await _context.ShoppingCarts
                                        .Include(x => x.CartDetails)
                                        .ThenInclude(x => x.Laptop)
                                        //for eager loading, x(Laptop?) should be not null here.
                                        .ThenInclude(x => x!.Brand)
                                        .Where(x => x.UserId == userId).FirstOrDefaultAsync();
            
             return shoppingCart;

        }

        public async Task<ShoppingCart?> GetShoppingCart(string userId) 
            => await _context.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);

        private string? GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal is not null)
                return _userManager.GetUserId(principal);
            else
                return null;
        }

    }
}