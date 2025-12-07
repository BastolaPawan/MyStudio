using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStudio.Models
{
    [Table("Payment")]

    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentNumber { get; set; } = string.Empty;

        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Card, Online, BankTransfer
        public string PaymentType { get; set; } = "Advance"; // Advance, Partial, Full, Refund

        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation
        public Order Order { get; set; } = null!;
    }
}
