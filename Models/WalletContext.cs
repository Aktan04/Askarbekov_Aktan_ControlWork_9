using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cw9.Models;

public class WalletContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public WalletContext(DbContextOptions<WalletContext> options) : base(options){}
    
    public DbSet<User> Users { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<ServiceProvider> ServiceProviders { get; set; }
    public DbSet<ServiceUser> ServiceUsers { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.FromUser)
            .WithMany(u => u.TransactionsFrom)
            .HasForeignKey(t => t.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.ToUser)
            .WithMany(u => u.TransactionsTo)
            .HasForeignKey(t => t.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<ServiceProvider>().HasData(
            new ServiceProvider { Id = 1, Name = "Operator A" },
            new ServiceProvider { Id = 2, Name = "Internet Provider B" }
        );

        modelBuilder.Entity<ServiceUser>().HasData(
            new ServiceUser { Id = 1, ServiceProviderId = 1, Identifier = "1234567890", Balance = 100 },
            new ServiceUser { Id = 2, ServiceProviderId = 2, Identifier = "0987654321", Balance = 150 }
        );
    }
}