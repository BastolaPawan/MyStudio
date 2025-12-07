namespace MyStudio.Models.DTOs
{
    public class ReceiveItemDto
    {
        public int PurchaseItemId { get; set; }
        public int QuantityReceived { get; set; }
        public decimal ActualUnitPrice { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
