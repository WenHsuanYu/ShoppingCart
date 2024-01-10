namespace ShoppingCartUI.Repositories
{
    public interface ICartRepository
    {
        Task<int> AddItem(int laptopId, int quantity);

        Task<int> GetCartItemCount(string userId = "");

        Task<ShoppingCart?> GetShoppingCart(string userId);

        Task<ShoppingCart?> GetUserCart();

        Task<bool> GoToCheckout();

        Task<int> RemoveItem(int laptopId);
    }
}