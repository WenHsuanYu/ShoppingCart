using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ShoppingCartUI.Repositories
{
    public class CartRepository : ICartRepository, IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;
        #region ctor
        public CartRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }
        #endregion
        #region AddItem
        //public async Task<bool> AddItem(int laptopId, int quantity)
        public async Task<int> AddItem(int laptopId, int quantity)
        {
            using var transaction = _context.Database.BeginTransaction();
            string userId = ((IUserRepository)this).GetUserId(_httpContextAccessor, _userManager);

            try
            {
                //string userId = GetUserId();
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
                var cartDetail = _context.CartDetails.FirstOrDefault(x => x.LaptopId == laptopId &&
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
                        LaptopId = laptopId,
                        Quantity = quantity,
                    };
                    _context.CartDetails.Add(cartDetail);
                }
                _context.SaveChanges();
                transaction.Commit();
                //return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                //return false;
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }
        #endregion
        #region GetCartItemCount
        public async Task<int> GetCartItemCount(string userId = "")
        {
            //if (!string.IsNullOrEmpty(userId))
            //{
            //    userId = GetUserId();
            //}
            var data = await (from cart in _context.ShoppingCarts
                              join cartDetail in _context.CartDetails
                              on cart.Id equals cartDetail.ShoppingCartId
                              select cartDetail.Quantity
                              ).ToListAsync();
            return data.Count != 0 ? data.Aggregate(func: (total, next) => total + next) : 0;
        }
        #endregion
        #region GetShoppingCart
        public async Task<ShoppingCart?> GetShoppingCart(string userId)
            => await _context.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
        #endregion
        #region GetUserCart
        public async Task<ShoppingCart?> GetUserCart()
        {
            string userId = ((IUserRepository)this).GetUserId(_httpContextAccessor, _userManager);
            if (string.IsNullOrEmpty(userId))
                return null;
            var shoppingCart = await _context.ShoppingCarts
                                        .Include(sc => sc.CartDetails)
                                        .ThenInclude(cd => cd.Laptop)
                                        //for eager loading, x(Laptop?) should be not null here.
                                        .ThenInclude(l => l!.ImageUrl)
                                        .Include(sc => sc.CartDetails)
                                        .ThenInclude(cd => cd.Laptop)
                                        .ThenInclude(l => l!.Brand)
                                        .Where(sc => sc.UserId == userId)
                                        .FirstOrDefaultAsync();

            return shoppingCart;
        }
        #endregion
        #region GoToCheckout
        public async Task<bool> GoToCheckout()
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                string userId = ((IUserRepository)this).GetUserId(_httpContextAccessor, _userManager);
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("The User is not logged in");
                var shoppingCart = await GetShoppingCart(userId) ?? throw new Exception("Invalid cart");
                var cartDetails = _context.CartDetails
                                        .Where(x => x.ShoppingCartId == shoppingCart.Id)
                                        .Include(x => x.Laptop).ToList();
                if (cartDetails.Count == 0)
                    throw new Exception("No items in the cart");
                var Order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    OrderStatusId = 1,
                    Status = "Pending",
                    Email = _userManager.Users.FirstOrDefault(x => x.Id == userId)!.Email,
                };
                _context.Orders.Add(Order);
                _context.SaveChanges();
                foreach (var item in cartDetails)
                {
                    var OrderDetail = new OrderDetail
                    {
                        OrderId = Order.Id,
                        LaptopId = item.LaptopId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Laptop!.Price
                    };
                    _context.OrderDetails.Add(OrderDetail);
                }
                _context.SaveChanges();
                _context.CartDetails.RemoveRange(cartDetails);
                _context.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
        #endregion
        #region RemoveItem
        public async Task<int> RemoveItem(int laptopId)
        {
            string userId = ((IUserRepository)this).GetUserId(_httpContextAccessor, _userManager);
            try
            {
                //string userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    throw new Exception("The User is not logged in");
                }
                var shoppingCart = await GetShoppingCart(userId) ?? throw new Exception("Invalid cart");
                // cart items detail
                var items = _context.CartDetails.FirstOrDefault(x => x.LaptopId == laptopId &&
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
                //return false;
            }
            //return true;
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }
        #endregion
    }
}