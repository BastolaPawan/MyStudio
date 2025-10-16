namespace MyStudio.Models
{
    public class InventoryReportViewModel
    {
        public List<Item> Items { get; set; } = new List<Item>();
        public List<Item> LowStockItems { get; set; } = new List<Item>();
        public List<Purchase> RecentPurchases { get; set; } = new List<Purchase>();
    }
}
