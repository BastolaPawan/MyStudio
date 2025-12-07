using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStudio.Models
{
    // Models/Customer.cs
    [Table("Customer")]
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string CustomerType { get; set; } = "Retail"; // Retail, Wholesale

        [Phone]
        public string? Phone { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Address { get; set; }

        // For wholesale customers
        public string? BusinessName { get; set; }
        public string? TaxNumber { get; set; }
        public decimal DiscountPercentage { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }

}
