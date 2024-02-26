using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ShoppingCartUI.Attributes;

namespace ShoppingCartUI.Models
{
    [Table("ImageUrl")]
    public class ImageUrl
    {
        [Key]
        public int Id {  get; set; }

        public required string DeleteHash { get; set; }
        [DataType(DataType.Url)]
        public required string? Url { get; set; }

        public int LaptopId { get; set; }

        public Laptop? Laptop { get; set; }
    }
}
