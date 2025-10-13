using Microsoft.EntityFrameworkCore;

namespace MyStudio.Models.DatabaseContext
{

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Loan> Loans { get; set; }
        public DbSet<LoanInstallment> LoanInstallments { get; set; }
        public DbSet<LoanTransaction> LoanTransactions { get; set; }
        public DbSet<InterestRateHistory> InterestRateHistory { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure your entities here if needed
        }
    }
}
