using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Cw9.Models;

public class User : IdentityUser<int>
{
    public int PersonalAccount { get; set; }
    public int Balance { get; set; }
    
    public ICollection<Transaction>? TransactionsFrom { get; set; }
    public ICollection<Transaction>? TransactionsTo { get; set; } 
}