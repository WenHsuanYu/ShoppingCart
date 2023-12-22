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
        [MaxLength(60)]
        public string? Processor { get; set; }

        [Required]
        public double Price { get; set; }
        public string? Image { get; set; }
        [Required]
        public int BrandId { get; set; }
#pragma warning disable CS8618
        public Brand Brand { get; set; }
        public List<CartDetail> CartDetail { get; set; }
        public List<OrderDetail> OrderDetail { get; set; }

        [NotMapped]
        public string BrandName { get; set; }
    }
}