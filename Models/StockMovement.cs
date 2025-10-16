namespace MyStudio.Models
{
    public class StockMovement
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public DateTime MovementDate { get; set; } = DateTime.Now;
        public string MovementType { get; set; } = string.Empty; // Purchase, Sale, Adjustment
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Property
        public Item Item { get; set; } = null!;
    }
}
