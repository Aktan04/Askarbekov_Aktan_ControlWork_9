using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cw9.Models;

public class WalletContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public WalletContext(DbContextOptions<WalletContext> options) : base(options){}
    
    public DbSet<User> Users { get; set; }

}