using System.ComponentModel.DataAnnotations;

namespace MyStudio.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }

        [Required]
        public string ItemType { get; set; } = "Service"; // Service, Product, Package

        [Required]
        public string Description { get; set; } = string.Empty;

        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal LineTotal { get; set; }

        // For products
        public int? ProductId { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;
    }
}
