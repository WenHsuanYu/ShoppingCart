namespace ShoppingCartUI.Repositories
{
    public interface ICartRepository
    {
        Task<bool> AddItem(int LaptopId, int quantity);
        Task<bool> RemoveItem(int LaptopId);
        Task<ShoppingCart?> GetUserCart();
        Task<ShoppingCart?> GetShoppingCart(string userId);
    }
}
