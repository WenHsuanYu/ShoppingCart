using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingCartUI.Models
{
    [Table("OrderDetail")]
    public class OrderDetail
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int LaptopId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public double UnitPrice { get; set; }

        //[Display(Name = "Status")]
        //[MaxLength(20, ErrorMessage = "Name cannot exceed 20 characters")]
        //public string? StatusName { get; set; } = string.Empty;

        public Order? Order { get; set; }
        public Laptop? Laptop { get; set; }
    }
}