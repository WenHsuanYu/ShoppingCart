﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingCartUI.Models
{
    [Table("OrderStatus")]
    public class OrderStatus
    {
        public int Id { get; set; }

        [Required]
        public int StatusId { get; set; }

        [Display(Name = "Status")]
        [Required]
        [MaxLength(20, ErrorMessage = "Name cannot exceed 20 characters")]
        public string? StatusName { get; set; }
    }
}