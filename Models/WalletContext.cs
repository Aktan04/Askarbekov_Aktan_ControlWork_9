using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cw9.Models;

public class WalletContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public WalletContext(DbContextOptions<WalletContext> options) : base(options){}
    
    public DbSet<User> Users { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

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
    }
}