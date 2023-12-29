using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingCartUI.Models
{
    [Table("CartDetail")]
    public class CartDetail
    {
        public int Id { get; set; }
        [Required]
        public int ShoppingCartId { get; set; }
        [Required]
        public int LaptopId { get; set; }
        [Required]
        public int Quantity { get; set; }
        public Laptop? Laptop { get; set; }
        public ShoppingCart? ShoppingCart { get; set; }
    }
}
