namespace ShoppingCartUI.Repositories
{
    public interface IUserOrderRepository
    {
        Task<IEnumerable<Order>> GetUserOrders();
    }
}