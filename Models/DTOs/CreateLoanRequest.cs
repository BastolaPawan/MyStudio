namespace MyStudio.Models.DTOs
{
    public class CreateLoanRequest
    {
        public string LoanAccountNumber { get; set; } = string.Empty;
        public string LoanType { get; set; } = string.Empty;
        public decimal InitialLoanAmount { get; set; }
        public DateTime StartDate { get; set; }
        public int LoanTenureYears { get; set; }
        public decimal InterestRate { get; set; }

        // Calculated fields (optional - can be calculated server-side)
        public decimal? InstallmentAmount { get; set; }
    }
}
