namespace ShoppingCartUI.Repositories
{
    public interface IHomeRepository
    {
        Task<IEnumerable<Laptop>> GetLaptopsAsync(string searchText = "", int BrandId = 0);
        Task<IEnumerable<Brand>> GetBrandsAsync();
    }
}