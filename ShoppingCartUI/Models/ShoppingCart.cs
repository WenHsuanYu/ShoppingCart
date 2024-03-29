﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingCartUI.Models
{
    [Table("ShoppingCart")]
    public class ShoppingCart
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
        public ICollection<CartDetail> CartDetails { get; set; } = null!;
    }
}
