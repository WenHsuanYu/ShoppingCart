using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using RestSharp;
using ShoppingCartUI.Data.Migrations;
//using ShoppingCartUI.Models;
namespace ShoppingCartUI.Repositories
{
    public class ImageUrlRepository : IImageUrlRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ImageUrlRepository> _logger;
        public ImageUrlRepository(ApplicationDbContext dbContext, ILogger<ImageUrlRepository> logger)
        {
            _context = dbContext;
            _logger = logger;
        }

        public async Task AddImageUrlModelAsync(string url, string hash, Laptop laptop)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                await _context.Laptops.AddAsync(laptop);
                _context.SaveChanges();
                ImageUrl imageurl = new ImageUrl
                {
                    Url = url,
                    DeleteHash = hash,
                    LaptopId = laptop.Id
                };
                await _context.ImageUrls.AddAsync(imageurl);
                _context.SaveChanges();
            }
            catch (OperationCanceledException e)
            {
                _logger.LogCritical($"{e.Message}");
            }
            catch (DbUpdateConcurrencyException e)
            {
                _logger.LogCritical($"{e.Message}");
            }
            catch (DbUpdateException e)
            {
                _logger.LogCritical($"{e.Message}");
            }
            catch (Exception e)
            {
                _logger.LogCritical($"{e.Message}");
            }
            finally
            {
                transaction.Commit();
            }
        }

        public async Task<string> DeleteImageUrlModelAsync(ImageUrl imageUrl)
        {
            var laptop = await _context.Laptops.Include(x => x.ImageUrl).FirstAsync(x => x.ImageUrl == imageUrl);
            string? hash = laptop?.ImageUrl?.DeleteHash;
            if (laptop is not null)
                _context.Remove(laptop);
            await _context.SaveChangesAsync();
            return hash;
        }
    }
}

