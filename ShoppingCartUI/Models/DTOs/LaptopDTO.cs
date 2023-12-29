namespace ShoppingCartUI.Models.DTOs
{
    public class LaptopDTO
    {
        public IEnumerable<Laptop> Laptops { get; set; } = null!;
        public IEnumerable<Brand> Brands { get; set; } = null!;
        public string SearchText { get; set; } = "";
        public int BrandId { get; set; } = 0;
    }
}
