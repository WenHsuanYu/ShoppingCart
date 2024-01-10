using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ShoppingCartUI.Repositories
{
    public class UserOrderRopository : IUserOrderRepository, IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        public UserOrderRopository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Order>> GetUserOrders()
        {
            var userId = ((IUserRepository)this).GetUserId(_httpContextAccessor, _userManager);
            if (string.IsNullOrEmpty(userId))
                throw new Exception("The User is not logged in");

            var orders = await _context.Orders
                                       .Include(x => x.OrderStatus)
                                       .Include(x => x.OrderDetail)
                                       .ThenInclude(x => x.Laptop)
                                       .ThenInclude(x => x!.Brand)
                                       .Where(x => x.UserId == userId)
                                       .ToListAsync();
            return orders;
        }
    }
}