namespace MyStudio.Models
{
    public class PurchaseItem
    {
        public int Id { get; set; }

        // Foreign Keys
        public int PurchaseId { get; set; }
        public int ItemId { get; set; }

        // Purchase Details
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal LineTotal { get; set; }

        // Can be edited later
        public decimal? ActualUnitPrice { get; set; }
        public int? QuantityReceived { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }

        // Navigation Properties
        public Purchase Purchase { get; set; } = null!;
        public Item Item { get; set; } = null!;
    }
}
