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
