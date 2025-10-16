using System.ComponentModel.DataAnnotations;

namespace MyStudio.Models
{
    public class Purchase
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string PurchaseNumber { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public DateTime? DeliveryDate { get; set; }

        // Foreign Key
        public int VendorId { get; set; }

        // Financials
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }

        public string? Notes { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Properties
        public Vendor Vendor { get; set; } = null!;
        public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    }
}
