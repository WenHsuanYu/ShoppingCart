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

        [Display(Name = "Order Date")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
        public int OrderStatusId { get; set; }
        public string? Status { get; set; }

        [RegularExpression(@"^.+@gmail\.com$", ErrorMessage = "Email must be gmail.com")]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        public OrderStatus? OrderStatus { get; set; }
        public List<OrderDetail> OrderDetail { get; set; } = [];
    }
}