using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingCartUI.Models
{
    [Table("Brands")]
    public class Brand
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(40, ErrorMessage = "Brand name cannot be longer than 40 characters.")]
        public string BrandName { get; set; }
        public List<Laptop> Laptops { get; set; }
    }
}
