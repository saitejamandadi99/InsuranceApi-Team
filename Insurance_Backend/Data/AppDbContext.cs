using Insurance_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Insurance_Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }
        public DbSet<User> User { get; set; }
        public DbSet<CustomerProfile> CustomerProfile { get; set; }
        public DbSet<InsuranceProduct> InsuranceProduct { get; set; }
        public DbSet<PolicyPlan> PolicyPlan { get; set; }
        public DbSet<Policy> Policy { get; set; }
        public DbSet<Claim> Claim { get; set; }
        public DbSet<PremiumPayment> PremiumPayment { get; set; }
        public DbSet<ClaimDocument> ClaimDocument { get; set; }
        public DbSet<ClaimStatusHistory> ClaimStatusHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PolicyPlan>().Property(p => p.CoverageAmount).HasPrecision(10, 2);
            modelBuilder.Entity<PolicyPlan>().Property(p => p.PremiumAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Policy>().Property(p => p.TotalPremiumPaid).HasPrecision(18, 2);
            modelBuilder.Entity<PremiumPayment>().Property(p => p.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<Claim>().Property(p => p.ClaimAmount).HasPrecision(18, 2);

            //Unique Constraints
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<InsuranceProduct>().HasIndex(p => p.ProductName).IsUnique();
            modelBuilder.Entity<Policy>().HasIndex(p => p.PolicyNumber).IsUnique();
            modelBuilder.Entity<Claim>().HasIndex(c => c.ClaimNumber).IsUnique();
            modelBuilder.Entity<PremiumPayment>().HasIndex(p => p.TransactionReference).IsUnique();

            //one to one user = customer
            modelBuilder.Entity<User>().HasOne(u => u.CustomerProfile).WithOne(c => c.User).HasForeignKey<CustomerProfile>(c => c.UserId);

            //InsuranceProduct -> policyPlan
            modelBuilder.Entity<InsuranceProduct>().HasMany(i => i.PolicyPlans).WithOne(p => p.InsuranceProduct).HasForeignKey(p => p.ProductId).OnDelete(DeleteBehavior.Restrict);

            //customer -> policy
            modelBuilder.Entity<CustomerProfile>().HasMany(c => c.Policies).WithOne(p => p.Customer).HasForeignKey(p => p.CustomerId).OnDelete(DeleteBehavior.Restrict);

            // PolicyPlan -> Policy
            modelBuilder.Entity<PolicyPlan>().HasMany(p => p.Policies).WithOne(p => p.PolicyPlan).HasForeignKey(p => p.PlanId).OnDelete(DeleteBehavior.Restrict);

            // Policy -> PremiumPayments
            modelBuilder.Entity<Policy>().HasMany(p => p.PremiumPayments).WithOne(pp => pp.Policy).HasForeignKey(pp => pp.PolicyId).OnDelete(DeleteBehavior.Cascade);

            // Policy -> Claims
            modelBuilder.Entity<Policy>().HasMany(p => p.Claims).WithOne(c => c.Policy).HasForeignKey(c => c.PolicyId).OnDelete(DeleteBehavior.Cascade);

            // Claim -> ClaimDocument
            modelBuilder.Entity<Claim>().HasMany(c => c.ClaimDocuments).WithOne(cd => cd.Claim).HasForeignKey(cd => cd.ClaimId).OnDelete(DeleteBehavior.Cascade);

            // Claim -> ClaimStatusHistory
            modelBuilder.Entity<Claim>().HasMany(c => c.ClaimStatusHistories).WithOne(ch => ch.Claim).HasForeignKey(ch => ch.ClaimId).OnDelete(DeleteBehavior.Cascade);

            // User -> ClaimStatusHistory
            modelBuilder.Entity<User>().HasMany(u => u.ClaimStatusHistories).WithOne(ch => ch.User).HasForeignKey(ch => ch.UpdatedBy).OnDelete(DeleteBehavior.Restrict);
        }
    }

}