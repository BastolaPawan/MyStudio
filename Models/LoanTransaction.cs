using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStudio.Models
{
    [Table("LoanTransaction")]
    public class LoanTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        [ForeignKey("Loan")]
        public int LoanId { get; set; }

        [ForeignKey("LoanInstallment")]
        public int? InstallmentId { get; set; }

        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = string.Empty; // EMI, Prepayment, LateFee

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrincipalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal InterestAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LateFeeAmount { get; set; }

        public string? PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public Loan? Loan { get; set; }
        public LoanInstallment? Installment { get; set; }
    }
}
