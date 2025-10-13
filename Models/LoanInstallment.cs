using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStudio.Models
{
    [Table("LoanInstallment")]
    public class LoanInstallment
    {
        [Key]
        public int InstallmentId { get; set; }

        [ForeignKey("Loan")]
        public int LoanId { get; set; }

        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal InstallmentAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrincipalComponent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal InterestComponent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OpeningBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ClosingBalance { get; set; }

        // Payment tracking
        public DateTime? PaidDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrincipalPaid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal InterestPaid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LateFee { get; set; }

        // Status
        public string InstallmentStatus { get; set; } = "Pending";
        public int DaysInPeriod { get; set; } // Number of days for this installment period

        // Navigation properties
        public Loan? Loan { get; set; }
        public List<LoanTransaction>? Transactions { get; set; }
    }
}
