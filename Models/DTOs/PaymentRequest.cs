namespace MyStudio.Models.DTOs
{
    public class PaymentRequest
    {
        public string LoanAccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public string? Remarks { get; set; }
    }
}
