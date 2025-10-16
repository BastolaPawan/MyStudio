using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStudio.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }

        // Foreign Keys
        public int UnitId { get; set; }
        public int ItemGroupId { get; set; }
        public int? ItemSubGroupId { get; set; }

        // Inventory Control
        public decimal MinimumStock { get; set; }
        public decimal MaximumStock { get; set; }
        public decimal ReorderLevel { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public Unit Unit { get; set; } = null!;
        public ItemGroup ItemGroup { get; set; } = null!;
        public ItemSubGroup? ItemSubGroup { get; set; }

        // Navigation Collections
        public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
        public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

        // Calculated Property for Current Stock
        [NotMapped] // This property is not stored in database
        public decimal CurrentStock => StockMovements?.Sum(sm => sm.Quantity) ?? 0;
    }

}
