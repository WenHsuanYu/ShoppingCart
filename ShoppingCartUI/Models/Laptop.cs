using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ShoppingCartUI.Attributes;

namespace ShoppingCartUI.Models
{
    [Table("Laptop")]
    public class Laptop
    {
        public int Id { get; set; }

        [Display(Name = "Model Name")]
        public string ModelName { get; set; } = string.Empty;

        [Required]
        [MaxLength(60)]
        public string? Processor { get; set; }

        [Required]
        public double Price { get; set; }

        [Display(Name = "Image")]
        public string? ImageFileName { get; set; }

        [Required]
        public int BrandId { get; set; }

        public Brand? Brand { get; set; }
        public List<CartDetail> CartDetail { get; set; } = new List<CartDetail>();
        public List<OrderDetail> OrderDetail { get; set; } = new List<OrderDetail>();

        [Display(Name = "Brand Name")]
        [NotMapped]
        public string? BrandName { get; set; }

        [NotMapped]
        [Display(Name = "Upload Image")]
        [DataType(DataType.Upload)]
        [MaxFileSize(3 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".jpeg", ".webp" })]
        public IFormFile? ImageFile { get; set; }

        public ImageUrl? ImageUrl { get; set; }
    }
}