using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStudio.Models
{
    [Table("Loan")]
    public class Loan
    {
        [Key]
        public int LoanId { get; set; }

        [Required]
        [MaxLength(50)]
        public string LoanAccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LoanType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal InitialLoanAmount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int LoanTenureYears { get; set; }

        [Column(TypeName = "decimal(8,4)")]
        public decimal CurrentInterestRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal InstallmentAmount { get; set; }

        // Dynamic fields
        public DateTime? LastInstallmentDate { get; set; }
        public DateTime FinalInstallmentDate { get; set; }
        public DateTime NextInstallmentDate { get; set; }
        public int InstallmentsPaidTillDate { get; set; }
        public int NoOfInstallmentsRemaining { get; set; }
        public int TotalInstallments { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OverDueAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OutstandingPrincipal { get; set; }

        // Status
        public string LoanStatus { get; set; } = "Active";
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        [MaxLength(20)]
        public string InterestCalculationType { get; set; } = "ActualDays"; // "EqualMonths" or "ActualDays"

        // Navigation properties
        public List<InterestRateHistory> InterestRateHistory { get; set; } = new();
        public List<LoanInstallment> Installments { get; set; } = new();
        public List<LoanTransaction> Transactions { get; set; } = new();
    }
}
