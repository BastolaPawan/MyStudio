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



        //Inventory Sections
        public DbSet<Unit> Units { get; set; }
        public DbSet<ItemGroup> ItemGroups { get; set; }
        public DbSet<ItemSubGroup> ItemSubGroups { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure your entities here if needed
        }
    }
}
