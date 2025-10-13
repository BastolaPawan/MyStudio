namespace MyStudio.Models.DTOs
{
    public class UpdateInterestRateRequest
    {
        public string LoanAccountNumber { get; set; } = string.Empty;
        public decimal NewInterestRate { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public string ChangedByUser { get; set; } = string.Empty;
        public string? ReasonForChange { get; set; }
    }
}
