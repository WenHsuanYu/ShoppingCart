namespace ShoppingCartUI.Repositories
{
    public interface IImageUrlRepository
    {
        Task AddImageUrlModelAsync(string url, string hash, Laptop laptop);
        Task<string> DeleteImageUrlModelAsync(ImageUrl imageUrl);
    }
}