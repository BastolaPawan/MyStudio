using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStudio.Models
{
    [Table("ProductService")]

    public class ProductService
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = "Service"; // Service, Product, Package
        public string? Category { get; set; } // Photography, Printing, Album, etc.
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }

        // For products
        public string? Unit { get; set; }
        public int? StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
