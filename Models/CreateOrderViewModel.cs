using System.ComponentModel.DataAnnotations;

namespace MyStudio.Models
{
    // ViewModels/CreateOrderViewModel.cs
    public class CreateOrderViewModel
    {
        [Required]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Order Type")]
        public string OrderType { get; set; } = "Studio";

        [Display(Name = "Session Type")]
        public string SessionType { get; set; } = string.Empty;

        [Display(Name = "Photo Shoot Date")]
        public DateTime? PhotoShootDate { get; set; }

        [Display(Name = "Expected Delivery Date")]
        public DateTime? DeliveryDate { get; set; }

        [Display(Name = "Delivery Method")]
        public string DeliveryMethod { get; set; } = "Pickup";

        // Courier details
        public string? CourierName { get; set; }
        public string? DeliveryAddress { get; set; }

        // Advance payment
        [Display(Name = "Advance Amount")]
        public decimal AdvanceAmount { get; set; }

        [Display(Name = "Payment Method")]
        public string AdvancePaymentMethod { get; set; } = "Cash";

        public string? Notes { get; set; }

        // Order items
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();

        // Dropdown options
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public List<ProductService> ProductsServices { get; set; } = new List<ProductService>();
    }

    public class OrderItemViewModel
    {
        public string ItemType { get; set; } = "Service";
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
    }

    // ViewModels/PaymentViewModel.cs
    public class PaymentViewModel
    {
        public int OrderId { get; set; }

        [Required]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Amount")]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "Cash";

        [Display(Name = "Reference Number")]
        public string? ReferenceNumber { get; set; }

        public string? Notes { get; set; }
    }

    // ViewModels/OrderSearchViewModel.cs
    public class OrderSearchViewModel
    {
        public string? OrderNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool ShowUnpaidOnly { get; set; }
        public bool ShowCancelled { get; set; }
        public bool ShowBadDebts { get; set; }
        public List<Order> Results { get; internal set; } = new List<Order>();
    }
}
