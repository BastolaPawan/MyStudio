using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStudio.Models
{
    [Table("Order")]
    public class Order
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string OrderNumber { get; set; } = string.Empty; // ORD-2024-001

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? DeliveryDate { get; set; }

        // Customer Information
        public int CustomerId { get; set; }

        // Order Details
        public string OrderType { get; set; } = "Studio"; // Studio, Outdoor, Product, Package
        public string SessionType { get; set; } = string.Empty; // Pre-wedding, Portrait, Family, etc.

        // Financials
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingCharge { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AdvancePaid { get; set; }
        //public decimal BalanceAmount => TotalAmount - AdvancePaid - Payments.Sum(p => p.Amount);
        [NotMapped]
        public decimal BalanceAmount { get; set; }

        // Delivery Information
        public string DeliveryMethod { get; set; } = "Pickup"; // Pickup, Courier, Delivery
        public string? CourierName { get; set; }
        public string? TrackingNumber { get; set; }
        public string? DeliveryAddress { get; set; }

        // Status Tracking
        public string Status { get; set; } = "Draft"; // Draft, Confirmed, InProgress, Ready, Delivered, Cancelled
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Partial, Paid, Overdue

        // Important Dates
        public DateTime? PhotoShootDate { get; set; }
        public DateTime? ReadyDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }

        // Cancellation & Bad Debt
        public bool IsCancelled { get; set; } = false;
        //Todo: Fix issue on CancelledDate.
        public DateTime? CancelledDate { get; set; }
        public string? CancellationReason { get; set; }
        public decimal CancellationCharge { get; set; } = 0;
        public bool IsBadDebt { get; set; } = false;

        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation
        public Customer Customer { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

}
