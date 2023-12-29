using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingCartUI.Models
{
    [Table("Orders")]
    public class Order
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; } = new string("");

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
        public int OrderStatusId { get; set; }
        public OrderStatus OrderStatus { get; set; } = null!;
        public List<OrderDetail> OrderDetail { get; set; } = new List<OrderDetail>();
    }
}
