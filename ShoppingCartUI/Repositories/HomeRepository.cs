using Microsoft.EntityFrameworkCore;
namespace ShoppingCartUI.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        private readonly ApplicationDbContext _dbcontext;
        public HomeRepository(ApplicationDbContext dbContext)
        {
            _dbcontext = dbContext;
        }
        public async Task<IEnumerable<Laptop>> GetLaptopsAsync(string searchText = "", int BrandId = 0)
        {
            searchText = searchText.ToLower();
            IEnumerable<Laptop> Laptops = await (from Laptop in _dbcontext.Laptops
                                                 join Brand in _dbcontext.Brands
                                                 on Laptop.BrandId equals Brand.Id
                                                 where string.IsNullOrWhiteSpace(searchText) || (Laptop != null && Laptop.ModelName.ToLower().StartsWith(searchText))
                                                 select new Laptop
                                                 {
                                                     Id = Laptop.Id,
                                                     ImageFileName = Laptop.ImageFileName,
                                                     ModelName = Laptop.ModelName,
                                                     Processor = Laptop.Processor,
                                                     Price = Laptop.Price,
                                                     BrandId = Laptop.BrandId,
                                                     BrandName = Brand.BrandName
                                                 }
                          ).ToListAsync();
            if (BrandId > 0)
            {
                Laptops = Laptops.Where(x => x.BrandId == BrandId).ToList();
            }
            return Laptops;
        }
        /// <summary>
        /// Get all brands from database
        /// </summary>
        /// <returns> a list of Brand</returns>
        public async Task<IEnumerable<Brand>> GetBrandsAsync()
        {
            return await _dbcontext.Brands.ToListAsync();
        }

    }
}
