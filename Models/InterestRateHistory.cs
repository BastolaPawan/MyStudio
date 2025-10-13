using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStudio.Models
{
    [Table("InterestRateHistory")]
    public class InterestRateHistory
    {
        [Key]
        public int InterestHistoryId { get; set; }

        [ForeignKey("Loan")]
        public int LoanId { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTill { get; set; }
        public string ChangedByUser { get; set; } = string.Empty;
        public string? ReasonForChange { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation property
        public Loan? Loan { get; set; }
    }

}
