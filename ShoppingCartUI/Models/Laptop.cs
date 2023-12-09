using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingCartUI.Models
{
    [Table("Laptop")]
    public class Laptop
    {
        public int Id { get; set; }
        public string ModelName { get; set; } = string.Empty;
        [Required]
        public double Price { get; set; }
        public string? Image { get; set; }
        [Required]
        public int BrandId { get; set; }
        #pragma warning disable CS8618
        public Brand Brand;

    }
}
